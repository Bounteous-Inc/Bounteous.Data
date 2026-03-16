using Bounteous.Data.Domain.Entities;
using Bounteous.Data.Domain.Interfaces;

namespace Bounteous.Data.Tests.Domain;

public class Project : AuditBase, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public ICollection<ProjectFile> Files { get; set; } = new List<ProjectFile>();
}
