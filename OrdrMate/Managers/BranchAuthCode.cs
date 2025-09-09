using Hangfire;
using OrdrMate.Repositories;
using OrdrMate.Utils;

namespace OrdrMate.Managers;

public class BranchAuthCode
{
    private readonly static Dictionary<string, string> _branchCodes = [];
    private readonly IBranchRepo _branchRepo;

    public BranchAuthCode(
        IBranchRepo branchRepo
    )
    {
        _branchRepo = branchRepo;
        Init();
    }

    private void Init()
    {
        var branches = _branchRepo.GetAllBranches().Result;
        foreach (var branch in branches)
        {
            if (!_branchCodes.ContainsKey(branch.Id))
            {
                _branchCodes[branch.Id] = RandomGenerator.GenerateNumericCode(6);
                RecurringJob.AddOrUpdate(branch.Id, () => RegenerateCode(branch.Id), "*/2 * * * *");
            }
        }
    }

    public string GetCode(string branchId)
    {
        if (_branchCodes.ContainsKey(branchId))
        {
            return _branchCodes[branchId];
        }
        else
        {
            var newCode = RandomGenerator.GenerateNumericCode(6);
            _branchCodes[branchId] = newCode;
            return newCode;
        }
    }

    public void RegenerateCode(string branchId)
    {
        var newCode = RandomGenerator.GenerateNumericCode(6);
        _branchCodes[branchId] = newCode;
    }

}