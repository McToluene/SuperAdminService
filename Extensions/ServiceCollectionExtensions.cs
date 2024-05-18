using Common.Services.Implentations;
using Common.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using SuperAdmin.Service.Database;
using SuperAdmin.Service.Database.Entities;
using SuperAdmin.Service.Helpers;
using SuperAdmin.Service.Models.Configurations;
using SuperAdmin.Service.Services.Contracts;
using SuperAdmin.Service.Services.Implementations;

namespace SuperAdmin.Service.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// This extension method is used to inject services
        /// </summary>
        /// <param name="services"></param>
        public static void InjectSuperAdminServices(this IServiceCollection services)
        {
            services.AddIdentity();
            services.AddTransient<UserManager<User>>();
            services.AddTransient<RoleManager<Role>>();
            
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IOtpService, OtpService>();
            
            services.AddTransient<ITokenService, TokenService>();
            services.AddTransient<IWeavrClientService, WeavrClientService>();
            
            services.AddSingleton<RestHttpClient>();
            services.AddTransient<IPermissionService, PermissionService>();
            
            services.AddTransient<IRoleService, RoleService>();
            services.AddTransient<IUserService, UserService>();
            
            services.AddTransient<ICountryService, CountryService>();
            services.AddTransient<ISalaryDisbursementChargeService, SalaryDisbursementChargeService>();
            
            services.AddTransient<IPayrollVendorChargeService, PayrollVendorChargeService>();
            services.AddTransient<IGCPBucketService, GCPBucketService>();
            
            services.AddTransient<ITicketCategoryService, TicketCategoryService>();
            services.AddTransient<ITicketService, TicketService>();
            
            services.AddTransient<IAutoSavingService, AutoSavingService>();
            services.AddTransient<ITenantService, TenantService>();

            services.AddTransient<ISubscriptionService, HttpSubscriptionService>();
            services.AddTransient<IProjectService, HttpProjectService>();

            services.AddTransient<IProductUpdatesService, ProductUpdatesService>();

            services.AddHttpContextAccessor();
            services.AddLogging();
        }

        public static void AddIdentity(this IServiceCollection services)
        {
            services.AddIdentity<User, Role>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        }

        public static void ConfigureAppSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<Jwt>(configuration.GetSection("Jwt"));
        }
    }
}
