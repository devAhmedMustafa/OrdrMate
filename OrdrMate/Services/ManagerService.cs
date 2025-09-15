using Microsoft.EntityFrameworkCore;
using OrdrMate.DTOs.User;
using OrdrMate.Middlewares;
using OrdrMate.Models;
using OrdrMate.Repositories;
using OrdrMate.Enums;

namespace OrdrMate.Services;

public class ManagerService(IUserRepo r, IPharmacyRepo rr, IConfiguration c, IBranchRepo branchRepo)
{
    private readonly IUserRepo _repo = r;
    private readonly IPharmacyRepo _PharmacyRepo = rr;
    private readonly IBranchRepo _branchRepo = branchRepo;
    private readonly IConfiguration _config = c;

    public async Task<IEnumerable<ManagerDTO>> GetAllManagers()
    {
        var managers = await _repo.GetAll();
        return managers.Select(m => new ManagerDTO
        {
            Id = m.Id,
            Username = m.Username,
            Role = m.Role
        });
    }

    public async Task<ManagerDTO> CreateManager(CreateManagerDTO dto)
    {

        try
        {
            var manager = new User
            {
                Username = dto.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = Enum.TryParse<UserRole>(dto.Role, true, out var role) ? role : UserRole.BranchManager
            };

            var createdManager = await _repo.CreateUser(manager);

            var managerDto = new ManagerDTO
            {
                Id = createdManager.Id,
                Username = createdManager.Username,
                Role = createdManager.Role
            };

            return managerDto;
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException!.Message.Contains("duplicate"))
                throw new Exception("Username already exists");
            throw;
        }
    }

    public async Task<LoginSuccessDto> AuthenticateManager(LoginDTO data)
    {

        var manager = await _repo.GetUserByUsername(data.Username)
            ?? throw new Exception("Invalid Credentials");

        if (!BCrypt.Net.BCrypt.Verify(data.Password, manager.Password))
            throw new Exception("Invalid Credentials");

        var jwtService = new JWTService(_config);

        var PharmacyId = "";
        var branchId = "";

        if (manager.Role == UserRole.TopManager)
        {
            var Pharmacy = await _PharmacyRepo.GetPharmacyByManagerId(manager.Id);
            if (Pharmacy != null)
            {
                PharmacyId = Pharmacy.Id;
                branchId = "HEAD";
            }
        }

        else if (manager.Role == UserRole.BranchManager)
        {
            var branch = await _branchRepo.GetBranchByManagerId(manager.Id);
            if (branch != null)
            {
                PharmacyId = branch.PharmacyId;
                branchId = branch.Id;
            }
        }

        return new LoginSuccessDto
        {
            Token = jwtService.GenerateJWT(manager.Id, manager.Role),
            Role = manager.Role.ToString(),
            PharmacyId = PharmacyId,
            BranchId = branchId
        };
    }

    public async Task<bool> IsUsernameTaken(string username)
    {
        return await _repo.IsUsernameTaken(username);
    }
}