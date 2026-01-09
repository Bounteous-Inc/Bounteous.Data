namespace Bounteous.Data.Tests.Context;

public class TestIdentityProvider<TUserId> : IIdentityProvider<TUserId> where TUserId : struct
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
