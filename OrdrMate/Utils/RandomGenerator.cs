namespace OrdrMate.Utils;

using System;
using System.Text;

public static class RandomGenerator
{
    private static readonly Random _random = new();

    public static string GenerateRandomString(int length, string baseName = "")
    {
        var stringBuilder = new StringBuilder(baseName);
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        for (var i = 0; i < length - baseName.Length; i++)
        {
            stringBuilder.Append(chars[_random.Next(chars.Length)]);
        }
        return stringBuilder.ToString();
    }

    public static string GenerateRandomPassword(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
        var passwordBuilder = new StringBuilder(length);
        for (var i = 0; i < length; i++)
        {
            passwordBuilder.Append(chars[_random.Next(chars.Length)]);
        }
        return passwordBuilder.ToString();
    }

    public static string GenerateNumericCode(int length)
    {
        var stringBuilder = new StringBuilder();
        const string digits = "0123456789";
        
        for (var i = 0; i < length; i++)
        {
            stringBuilder.Append(digits[_random.Next(digits.Length)]);
        }

        return stringBuilder.ToString();
    }

}