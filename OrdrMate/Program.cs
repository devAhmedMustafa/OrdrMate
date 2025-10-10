using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.FileProviders;
using OrdrMate.Middlewares;
using Hangfire;
using OrdrMate.Core;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var modules = typeof(Program).Assembly.GetTypes()
    .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
    .Select(Activator.CreateInstance).Cast<IModule>();

foreach (var module in modules)
{
    module.Register(builder.Services, builder.Configuration, env);
}

// CORS
builder.Services.AddCors(options =>
{
    // Allow all origins, methods, and headers
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        if (env.IsDevelopment())
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        }
        else if (env.IsProduction())
        {
            builder.WithOrigins(
                "https://gcm-manager-psi.vercel.app", "https://greencitymed.vercel.app")
                .AllowAnyMethod()
                .AllowAnyHeader();
        }
    });
});

builder.Services.AddControllers();


// Authorization
builder.Services.AddAuthorization();

builder.Services.Configure<AuthorizationOptions>(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.Requirements.Add(new AdminRequirement()));
});

builder.Services.AddScoped<IAuthorizationHandler, AdminHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHangfireDashboard();
}

var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
});

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowSpecificOrigin");

app.UseWebSockets();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
