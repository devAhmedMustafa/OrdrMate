using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OrdrMate.Features.Orders.ShareReservation.Middlewares;

public class TableReservationJwtMiddleware(IConfiguration c)
{

    private readonly IConfiguration _config = c;

    public string GenerateJWT(string reservationId)
    {

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["ShareReservationJwt:Key"]!));
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["ShareReservationJwt:Issuer"],
            audience: _config["ShareReservationJwt:Audience"],
            claims: [
                new Claim("reservationId", reservationId)
            ],
            expires: DateTime.Now.AddMinutes(15),
            notBefore: DateTime.Now,
            signingCredentials: cred
        );

        return new JwtSecurityTokenHandler().WriteToken(token);

    }

    public bool ValidateJWT(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["ShareReservationJwt:Key"]!);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["ShareReservationJwt:Issuer"],
                ValidAudience = _config["ShareReservationJwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            return true;
        }
        catch (SecurityTokenExpiredException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }
}