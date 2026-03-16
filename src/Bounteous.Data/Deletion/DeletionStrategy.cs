using Bounteous.Data.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Bounteous.Data.Deletion;

/// <summary>
/// Handles cascade deletion logic for entities with different deletion strategies.
/// Ensures that when a parent entity is deleted, child entities are handled appropriately
/// based on their deletion strategy markers (ISoftDelete or IHardDelete).
/// </summary>
public class DeletionStrategy
{
    /// <summary>
    /// Applies cascade soft delete logic to child entities when their parent is being soft deleted.
    /// This prevents orphaned records by ensuring children are also marked as deleted.
    /// </summary>
    /// <param name="changeTracker">The EF Core change tracker containing entity entries</param>
    public void ApplyCascadeSoftDelete(ChangeTracker changeTracker)
    {
        // Find all parent entities that are being soft deleted
        var softDeletedParents = changeTracker
            .Entries()
            .Where(e => e is { Entity: ISoftDelete, State: EntityState.Deleted })
            .Where(e => !((ISoftDelete)e.Entity).IsDeleted) // Only entities being soft deleted (not already marked)
            .ToList();

        foreach (var parentEntry in softDeletedParents)
        {
            CascadeSoftDeleteToChildren(parentEntry);
        }
    }

    /// <summary>
    /// Recursively handles child entities when a parent is soft deleted.
    /// - Children with ISoftDelete are soft deleted
    /// - Children with IHardDelete are physically deleted
    /// </summary>
    /// <param name="parentEntry">The parent entity entry being deleted</param>
    private void CascadeSoftDeleteToChildren(EntityEntry parentEntry)
    {
        // Get all navigation properties that are collections (one-to-many relationships)
        var collectionNavigations = parentEntry.Metadata.GetNavigations()
            .Where(n => n.IsCollection);

        foreach (var navigation in collectionNavigations)
        {
            // Load the collection if not already loaded
            var collectionEntry = parentEntry.Collection(navigation.Name);
            if (!collectionEntry.IsLoaded)
            {
                collectionEntry.Load();
            }

            var children = collectionEntry.CurrentValue;
            if (children == null) continue;

            // Handle each child based on its deletion strategy
            foreach (var child in children)
            {
                var childEntry = parentEntry.Context.Entry(child);
                
                // Skip if already being deleted
                if (childEntry.State == EntityState.Deleted)
                    continue;
                
                if (child is ISoftDelete softDeleteChild && !softDeleteChild.IsDeleted)
                {
                    // Soft delete children with ISoftDelete
                    softDeleteChild.IsDeleted = true;
                    
                    // If the child is unchanged, mark it as modified so the soft delete is persisted
                    if (childEntry.State == EntityState.Unchanged)
                    {
                        childEntry.State = EntityState.Modified;
                    }
                    
                    // Recursively cascade to grandchildren
                    CascadeSoftDeleteToChildren(childEntry);
                }
                else if (child is IHardDelete)
                {
                    // Physically delete children with IHardDelete
                    // Mark as deleted so EF Core will remove them from the database
                    childEntry.State = EntityState.Deleted;
                    
                    // Recursively cascade to grandchildren
                    CascadeSoftDeleteToChildren(childEntry);
                }
            }
        }
    }

    /// <summary>
    /// Gets all entities that should be physically deleted (bypassing soft delete).
    /// This includes entities with IHardDelete marker and entities explicitly marked for physical deletion.
    /// </summary>
    /// <param name="changeTracker">The EF Core change tracker containing entity entries</param>
    /// <returns>Collection of entity entries that should be physically deleted</returns>
    public IEnumerable<EntityEntry> GetPhysicalDeleteEntries(ChangeTracker changeTracker)
    {
        return changeTracker
            .Entries()
            .Where(e => e.State == EntityState.Deleted)
            .Where(e => ShouldPhysicallyDelete(e));
    }

    /// <summary>
    /// Determines if an entity should be physically deleted from the database.
    /// </summary>
    /// <param name="entry">The entity entry to check</param>
    /// <returns>True if the entity should be physically deleted, false if it should be soft deleted</returns>
    private bool ShouldPhysicallyDelete(EntityEntry entry)
    {
        // Entities with IHardDelete are always physically deleted
        if (entry.Entity is IHardDelete)
        {
            return true;
        }

        // Entities with ISoftDelete that are already marked as deleted should be physically deleted
        // This allows the Delete() extension method to force physical deletion
        if (entry.Entity is ISoftDelete softDelete && softDelete.IsDeleted)
        {
            return true;
        }

        return false;
    }
}
