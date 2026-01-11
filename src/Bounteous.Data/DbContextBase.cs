using Bounteous.Core.Extensions;
using Bounteous.Data.Audit;
using Bounteous.Data.Converters;
using Bounteous.Data.Domain.Interfaces;
using Bounteous.Data.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data;

public interface IDbContext<TUserId> where TUserId : struct
{
    IDbContext<TUserId> WithUserId(TUserId userId);
}

public abstract class DbContextBase<TUserId> : DbContext, IDbContext<TUserId>
    where TUserId : struct
{
    private readonly AuditVisitor<TUserId> auditVisitor;
    private readonly IDbContextObserver? observer;
    private readonly IIdentityProvider<TUserId> identityProvider;
    private TUserId TokenUserId { get; set; }

    protected DbContextBase(
        DbContextOptions options, 
        IDbContextObserver? observer, 
        IIdentityProvider<TUserId> identityProvider)
        : base(options)
    {
        auditVisitor = new AuditVisitor<TUserId>();
        this.observer = observer;
        this.identityProvider = identityProvider;
        
        if (this.observer == null) return;

        base.ChangeTracker.Tracked += this.observer.OnEntityTracked!;
        base.ChangeTracker.StateChanged += this.observer.OnStateChanged;
    }

    IDbContext<TUserId> IDbContext<TUserId>.WithUserId(TUserId userId)
    {
        TokenUserId = userId;
        return this;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        RegisterModels(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    public override void Dispose()
    {
        DisposeObserver();
        base.Dispose();
    }

    public override ValueTask DisposeAsync()
    {
        DisposeObserver();
        return base.DisposeAsync();
    }

    private void DisposeObserver()
    {
        if (observer == null) return;
        
        base.ChangeTracker.Tracked -= observer.OnEntityTracked!;
        base.ChangeTracker.StateChanged -= observer.OnStateChanged;
        observer.Dispose();
    }

    protected abstract void RegisterModels(ModelBuilder modelBuilder);

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<DateTimeConverter>();
        base.ConfigureConventions(configurationBuilder);
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ValidateReadOnlyEntities();
        ApplyAuditVisitor();
        var saved = await base.SaveChangesAsync(cancellationToken);
        observer?.OnSaved();
        return saved;
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ValidateReadOnlyEntities();
        ApplyAuditVisitor();
        var saved = base.SaveChanges(acceptAllChangesOnSuccess);
        observer?.OnSaved();
        return saved;
    }

    private void ValidateReadOnlyEntities()
    {
        // Skip validation if explicitly suppressed (e.g., for test data seeding)
        if (ReadOnlyValidationScope.IsSuppressed)
        {
            return;
        }

        var readOnlyViolations = ChangeTracker
            .Entries()
            .Where(e => e.Entity.GetType().GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyEntity<>)))
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        if (readOnlyViolations.Any())
        {
            var violation = readOnlyViolations.First();
            var operation = violation.State switch
            {
                EntityState.Added => "create",
                EntityState.Modified => "update",
                EntityState.Deleted => "delete",
                _ => "modify"
            };
            throw new ReadOnlyEntityException(violation.Entity.GetType().Name, operation);
        }
    }

    private void ApplyAuditVisitor()
    {
        var userId = GetEffectiveUserId();
        
        ChangeTracker
            .Entries()
            .Where(e => e is { Entity: IAuditableMarker<TUserId>, State: EntityState.Added })
            .ForEach(x => auditVisitor.AcceptNew(x, userId));

        ChangeTracker
            .Entries()
            .Where(e => e is { Entity: IAuditableMarker<TUserId>, State: EntityState.Modified })
            .ForEach(x => auditVisitor.AcceptModified(x, userId));

        ChangeTracker
            .Entries()
            .Where(e => e is { Entity: IDeleteable, State: EntityState.Deleted })
            .ForEach(x => auditVisitor.AcceptDeleted(x, userId));
    }

    private TUserId? GetEffectiveUserId()
    {
        if (!EqualityComparer<TUserId>.Default.Equals(TokenUserId, default))
            return TokenUserId;

        var userId = identityProvider.GetCurrentUserId();
        if (!EqualityComparer<TUserId>.Default.Equals(userId, default))
            return userId;

        return null;
    }
}