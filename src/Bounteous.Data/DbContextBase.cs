using System.Linq.Expressions;
using Bounteous.Core.Extensions;
using Bounteous.Data.Audit;
using Bounteous.Data.Converters;
using Bounteous.Data.Domain.Interfaces;
using Bounteous.Data.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data;

public interface IDbContext<in TUserId> where TUserId : struct
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
        ValidateDeletionMarkers(modelBuilder);
        ApplySoftDeleteQueryFilters(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    private void ValidateDeletionMarkers(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var isSoftDelete = typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType);
            var isHardDelete = typeof(IHardDelete).IsAssignableFrom(entityType.ClrType);
            
            if (isSoftDelete && isHardDelete)
            {
                throw new InvalidOperationException(
                    $"Entity '{entityType.ClrType.Name}' cannot implement both ISoftDelete and IHardDelete. " +
                    "Choose one deletion strategy per entity.");
            }

            var isAuditBase = typeof(IAuditableMarker<TUserId>).IsAssignableFrom(entityType.ClrType);
            if (isAuditBase && !isSoftDelete && !isHardDelete)
            {
                throw new InvalidOperationException(
                    $"Entity '{entityType.ClrType.Name}' inherits from AuditBase but does not implement ISoftDelete or IHardDelete. " +
                    "All auditable entities must explicitly choose a deletion strategy.");
            }
        }
    }

    private void ApplySoftDeleteQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                var filter = Expression.Lambda(Expression.Equal(property, Expression.Constant(false)), parameter);
                
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
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
    
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ValidateReadOnlyRequest();
        ValidateReadOnlyEntities();
        ApplyAuditVisitor();
        var saved = base.SaveChanges(acceptAllChangesOnSuccess);
        observer?.OnSaved();
        return saved;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ValidateReadOnlyRequest();
        ValidateReadOnlyEntities();
        ApplyAuditVisitor();
        var saved = await base.SaveChangesAsync(cancellationToken);
        observer?.OnSaved();
        return saved;
    }

    private void ValidateReadOnlyRequest()
    {
        if (ReadOnlyRequestScope.IsActive)
        {
            var modifiedEntities = ChangeTracker
                .Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .Select(e => new { e.Entity.GetType().Name, e.State })
                .ToList();

            if (modifiedEntities.Any())
            {
                var entityDetails = string.Join(", ", 
                    modifiedEntities.Select(e => $"{e.Name} ({e.State})"));
                
                throw new InvalidOperationException(
                    $"Cannot save changes within a read-only request scope. " +
                    $"This operation is marked as query-only. " +
                    $"Modified entities: {entityDetails}");
            }

            throw new InvalidOperationException(
                "Cannot save changes within a read-only request scope. " +
                "This operation is marked as query-only.");
        }
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
            .Where(e => e is { Entity: ISoftDelete, State: EntityState.Deleted })
            .Where(e => !((ISoftDelete)e.Entity).IsDeleted) // Skip if already marked deleted (physical delete)
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