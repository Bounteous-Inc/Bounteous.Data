namespace Bounteous.Data;

public interface IIdentityProvider<TUserId> where TUserId : struct
{
    TUserId GetCurrentUserId();
}

public class IdentityProvider<TUserId> : IIdentityProvider<TUserId> where TUserId : struct
{
    private TUserId currentUserId;

    public TUserId GetCurrentUserId() => currentUserId;

    public void SetCurrentUserId(TUserId userId)
    {
        currentUserId = userId;
    }

    public void ClearCurrentUserId()
    {
        currentUserId = default;
    }
}