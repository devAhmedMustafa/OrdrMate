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
}