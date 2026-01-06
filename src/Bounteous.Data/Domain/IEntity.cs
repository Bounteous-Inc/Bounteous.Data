namespace Bounteous.Data.Domain;

public interface IEntity<TId>
{
    TId Id { get; set; }
}
