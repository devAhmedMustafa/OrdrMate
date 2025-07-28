using Microsoft.AspNetCore.Authorization;

namespace OrderMate.Middlewares;

public class IsOpenRequirement : IAuthorizationRequirement {}