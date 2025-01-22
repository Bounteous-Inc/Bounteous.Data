using Bounteous.DotNet.Data.Audit;
using Bounteous.DotNet.Data.Domain;
using Microsoft.EntityFrameworkCore;
using Xerris.DotNet.Core.Extensions;

namespace Bounteous.DotNet.Data;

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
        ApplyAuditVisitor();
        var saved = await base.SaveChangesAsync(cancellationToken);
        observer?.OnSaved();
        return saved;
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditVisitor();
        var saved = base.SaveChanges(acceptAllChangesOnSuccess);
        observer?.OnSaved();
        return saved;
    }

    private void ApplyAuditVisitor()
    {
        ChangeTracker
            .Entries()
            .Where(e => e is { Entity: IAuditable, State: EntityState.Added })
            .ForEach(x => auditVisitor.AcceptNew(x, TokenUserId));

        ChangeTracker
            .Entries()
            .Where(e => e is { Entity: IAuditable, State: EntityState.Modified })
            .ForEach(x => auditVisitor.AcceptModified(x, TokenUserId));

        //deleted entities
        ChangeTracker
            .Entries()
            .Where(e => e is { Entity: IDeleteable, State: EntityState.Deleted })
            .ForEach(x => auditVisitor.AcceptDeleted(x, TokenUserId));
    }
}