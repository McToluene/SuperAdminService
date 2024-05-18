using Configurations;
using Configurations.Extensions.Middlewares;
using Configurations.Extensions.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Converters;
using SuperAdmin.Service.Database;
using SuperAdmin.Service.Database.Entities;
using SuperAdmin.Service.Extensions;
using SuperAdmin.Service.Services.GrpcServices.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

// Register context
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(DatabaseConnectionString.ConnectionString);
});

builder.Services.ConfigureAppSettings(builder.Configuration);
builder.Services.AddAuthentication(builder.Configuration);
builder.Services.InjectServices(builder.Configuration);
builder.Services.InjectSuperAdminServices();

builder.Services.AddGrpc();
builder.Services.AddControllers()
        .AddNewtonsoftJson(options => options.SerializerSettings.Converters.Add(new StringEnumConverter()));
builder.Services.AddSwaggerGenNewtonsoftSupport();

IServiceProvider serviceProvider = builder.Services.BuildServiceProvider();
var context = serviceProvider.GetRequiredService<AppDbContext>();
var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
await DataSeeder.Seed(context, userManager);

var app = builder.Build();

// Map Grpc Service
app.MapGrpcService<SalaryDisbursementService>();

// Configure the HTTP request pipeline.
app.InjectMiddlewares();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
