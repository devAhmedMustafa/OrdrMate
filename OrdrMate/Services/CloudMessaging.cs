using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using OrdrMate.Data;
using OrdrMate.Models;

namespace OrdrMate.Services;

public class CloudMessaging
{
    private readonly OrdrMateDbContext _db;

    public CloudMessaging(OrdrMateDbContext db)
    {
        _db = db;
    }

    public async Task<string> GetTokenByUserId(string userId)
    {
        var token = await _db.FirebaseToken
            .Where(t => t.UserId == userId)
            .Select(t => t.Token)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(token))
        {
            throw new KeyNotFoundException($"No Firebase token found for user with ID {userId}.");
        }

        return token;
    }

    public async Task<string> AssignTokenToUserAsync(string userId, string token)
    {
        var existingToken = await _db.FirebaseToken
            .FirstOrDefaultAsync(t => t.UserId == userId);

        if (existingToken != null)
        {
            _db.FirebaseToken.Remove(existingToken);
        }

        var newToken = new FirebaseToken
        {
            UserId = userId,
            Token = token
        };
        
        await _db.FirebaseToken.AddAsync(newToken);

        await _db.SaveChangesAsync();
        return token;
    }

    public async Task SendNotificationAsync(string token, string title, string body)
    {
        var message = new Message()
        {
            Token = token,
            Notification = new Notification
            {
                Title = title,
                Body = body
            }
        };

        string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        Console.WriteLine($"Successfully sent message: {response}");
    }
}