namespace OrdrMate.Services;

using Google.Apis.Auth;
using OrdrMate.DTOs.User;
using OrdrMate.Models;
using OrdrMate.Repositories;
using OrdrMate.Middlewares;

public class CustomerService
{
    private readonly IUserRepo _userRepo;
    private readonly IConfiguration _configuration;
    public CustomerService(IUserRepo userRepo, IConfiguration configuration)
    {
        _userRepo = userRepo;
        _configuration = configuration;
    }

    public async Task<GoogleLoginCredentialsDto> AuthenticateCustomer(GoogleLoginRequestDto dto)
    {

        GoogleJsonWebSignature.Payload? validPayload = null;

        try
        {
            validPayload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_configuration["Authentication:Google:ClientId"]]
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error validating Google ID token: {ex.Message}");
            throw new Exception("Error validating Google ID token", ex);
        }

        if (validPayload == null) throw new Exception("Invalid Google ID token");

        var user = await _userRepo.GetUserByUsername(validPayload.Email);
        if (user == null)
        {
            user = new User
            {
                Username = validPayload.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                Role = Enums.UserRole.Customer,
            };

            user = await _userRepo.CreateUser(user);
        }

        var jwtService = new JWTService(_configuration);
        var token = jwtService.GenerateJWT(user.Id, user.Role);
        if (token == null) throw new Exception("Error generating JWT token");

        return new GoogleLoginCredentialsDto
        {
            Token = token,
            Email = user.Username,
            UserId = user.Id
        };
    }
}