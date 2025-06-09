using OrdrMate.Models;

namespace OrdrMate.Events;

public static class BranchEvents
{
    public static event Action<Branch>? BranchCreated;
    public static event Action<Branch>? BranchDeleted;
    public static event Action<string>? BranchSocketConnected;
    public static event Action<string, string, int>? KitchenUpdate;

    public static void OnBranchCreated(Branch branch)
    {
        BranchCreated?.Invoke(branch);
    }

    public static void OnBranchDeleted(Branch branch)
    {
        BranchDeleted?.Invoke(branch);
    }

    public static void OnBranchSocketConnected(string branchId)
    {
        BranchSocketConnected?.Invoke(branchId);
    }

    public static void OnKitchenUpdate(string branchId, string kitchenId, int kitchenPower)
    {
        KitchenUpdate?.Invoke(branchId, kitchenId, kitchenPower);
    }
}