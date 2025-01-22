using Bounteous.DotNet.Data.Domain;

namespace Bounteous.DotNet.Data.Exceptions;

public class NotFoundException<T>(Guid id) : Exception where T : class, IAuditable
{
    public override string Message =>  $"{typeof(T).Name} with id: {id} not found";
}