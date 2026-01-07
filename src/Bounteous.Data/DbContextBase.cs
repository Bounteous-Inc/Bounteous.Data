using Bounteous.Core.Extensions;
using Bounteous.Data.Audit;
using Bounteous.Data.Converters;
using Bounteous.Data.Domain;
using Bounteous.Data.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data;

public interface IDbContext
{
    DbContext WithUserId(Guid userId);
}

public abstract class DbContextBase : DbContext, IDbContext, IDisposable
{
    private readonly AuditVisitor auditVisitor;
    private readonly IDbContextObserver? observer;
    private Guid? TokenUserId { get; set; }

    protected DbContextBase(DbContextOptions<DbContextBase> options, IDbContextObserver observer)
        : base(options)
    {
        auditVisitor = new AuditVisitor();
        this.observer = observer;
        if (this.observer == null) return;

        base.ChangeTracker.Tracked += this.observer.OnEntityTracked!;
        base.ChangeTracker.StateChanged += this.observer.OnStateChanged;
    }

    public DbContext WithUserId(Guid userId)
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
        ChangeTracker
            .Entries()
            .Where(e => e is { Entity: IAuditableMarker, State: EntityState.Added })
            .ForEach(x => auditVisitor.AcceptNew(x, TokenUserId));

        ChangeTracker
            .Entries()
            .Where(e => e is { Entity: IAuditableMarker, State: EntityState.Modified })
            .ForEach(x => auditVisitor.AcceptModified(x, TokenUserId));

        //deleted entities
        ChangeTracker
            .Entries()
            .Where(e => e is { Entity: IDeleteable, State: EntityState.Deleted })
            .ForEach(x => auditVisitor.AcceptDeleted(x, TokenUserId));
    }
}