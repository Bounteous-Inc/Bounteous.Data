namespace Bounteous.Data;

public interface IIdentityProvider<TUserId> where TUserId : struct
{
    TUserId? GetCurrentUserId();
}
