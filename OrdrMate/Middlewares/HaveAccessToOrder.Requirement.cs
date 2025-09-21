using Microsoft.AspNetCore.Authorization;

namespace OrdrMate.Middlewares;

public class HaveAccessToOrderRequirement : IAuthorizationRequirement{}