using Bounteous.Data.Domain.Interfaces;

namespace Bounteous.Data.Exceptions;

public class NotFoundException<T, TId>(TId id) : Exception where T : class, IEntity<TId>
{
    public override string Message =>  $"{typeof(T).Name} with id: {id} not found";
}

public class NotFoundException<T>(Guid id) : NotFoundException<T, Guid>(id) 
                where T : class, IEntity<Guid>;