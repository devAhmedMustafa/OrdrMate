using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using OrdrMate.Data;
using OrdrMate.Middlewares;
using OrdrMate.Repositories;
using OrdrMate.Services;
using OrdrMate.Managers;
using OrdrMate.Sockets;
using Google.Apis.Auth.OAuth2;
using Hangfire;
using Hangfire.MemoryStorage;
using OrdrMate.Configs;
using MongoDB.Driver;
using OrdrMate.Core;
using Microsoft.EntityFrameworkCore;
using OrdrMate.Utils;

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

// PostgreSQL Database Context
builder.Services.AddDbContext<OrdrMateDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MongoDB Configuration
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));
builder.Services.AddSingleton<IMongoClient, MongoClient>(sp =>
{
    var settings = builder.Configuration.GetSection("MongoDb").Get<MongoDbSettings>();
    return new MongoClient(settings?.ConnectionString);
});

// MongoDB Context
builder.Services.AddScoped<OrdrMateMongoContext>();



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
                "https://ordrmate-manager.vercel.app", "https://gahezz.vercel.app")
                .AllowAnyMethod()
                .AllowAnyHeader();
        }
    });
});

// Repositories and Services

// builder.Services.AddScoped<IUserRepo, UserRepo>();
// builder.Services.AddScoped<ManagerService, ManagerService>();
// builder.Services.AddScoped<CustomerService, CustomerService>();

// builder.Services.AddScoped<IRestaurantRepo, RestaurantRepo>();
// builder.Services.AddScoped<RestaurantService, RestaurantService>();

// builder.Services.AddScoped<IItemRepo, ItemRepo>();
// builder.Services.AddScoped<ItemService, ItemService>();

// builder.Services.AddScoped<IBranchRepo, BranchRepo>();
// builder.Services.AddScoped<BranchService, BranchService>();
// builder.Services.AddScoped<IBranchRequestRepo, BranchRequestRepo>();

// builder.Services.AddScoped<ITableRepo, TableRepo>();
// builder.Services.AddScoped<TableService, TableService>();

// builder.Services.AddScoped<IKitchenRepo, KitchenRepo>();
// builder.Services.AddScoped<KitchenService, KitchenService>();


// builder.Services.AddScoped<IOrderRepo, OrderRepo>();
// builder.Services.AddScoped<OrderService, OrderService>();
// builder.Services.AddScoped<IDeliverRequestRepo, DeliverRequestRepo>();
// builder.Services.AddScoped<IPaymentRepo, PaymentRepo>();
// builder.Services.AddScoped<PaymentService, PaymentService>();

// builder.Services.AddScoped<ICustomizationRepo, CustomizationRepo>();
// builder.Services.AddScoped<CustomizationService, CustomizationService>();

// builder.Services.AddScoped<GeoMaps>();

builder.Services.AddScoped<CloudMessaging>();

// Sockets
builder.Services.AddScoped<BranchSocketHandler>();
builder.Services.AddScoped<CustomerOrdersSocketHandler>();

// Managers
// builder.Services.AddScoped<OrderManager>();
builder.Services.AddScoped<TableManager>();

// Third-party services
builder.Services.AddHttpClient<PaymobService>();
builder.Services.AddHttpClient<AiService>();
builder.Services.AddScoped<S3Service>();
// Hangfire
builder.Services.AddHangfire(config =>
{
    config.UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseMemoryStorage();
});

builder.Services.AddHangfireServer();

builder.Services.AddScoped<IBackgroundJobClient, BackgroundJobClient>();

builder.Services.AddControllers();

// JWT Authentication

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),
            RoleClaimType = ClaimTypes.Role,
        };
    });

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageRestaurant", policy =>
        policy.Requirements.Add(new ManageRestaurantRequirement()));

    options.AddPolicy("Admin", policy =>
        policy.Requirements.Add(new AdminRequirement()));

    options.AddPolicy("BranchManager", policy =>
        policy.Requirements.Add(new BranchManagerRequirement()));
});

// Authorization Handlers
builder.Services.AddScoped<IAuthorizationHandler, ManageRestaurantHandler>();
builder.Services.AddScoped<IAuthorizationHandler, AdminHandler>();
builder.Services.AddScoped<IAuthorizationHandler, BranchManagerHandler>();

FirebaseAdmin.FirebaseApp.Create(new FirebaseAdmin.AppOptions()
{
    Credential = GoogleCredential.FromFile("Keys/firebase-adminsdk.json"),
});

var app = builder.Build();
app.UseCors("AllowSpecificOrigin");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHangfireDashboard();
}

app.UseWebSockets();
app.UseAuthentication();
app.UseAuthorization();

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
app.MapControllers();
app.Run();

