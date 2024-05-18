using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SuperAdmin.Service.Database.Entities;
using SuperAdmin.Service.Helpers;
using SuperAdmin.Service.Models.Enums;

namespace SuperAdmin.Service.Database
{
    public class DataSeeder
    {
        public static async Task Seed(AppDbContext context, UserManager<User> userManager)
        {
            if (context.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
            {
                context.Database.Migrate();
            }

            await SeedCountries(context);
            await SeedPermissions(context);
            await SeedSuperAdminRole(context);
            await SeedSuperAdminUsers(context, userManager);
            await SeedSuspensionRole(context);
            await SeedRuleEngineForSalaryDisbursementCharge(context);
            await SeedPaysCompaniesForSalaryDisbursementCharge(context);
            await SeedPayrollVendorChargeDeductionBasis(context);
            await SeedPayrollVendorPercentsgeCharge(context);
            await SeedAutoSavingSettings(context);
            await SeedProductUpdateCategory(context);
            await SeedSupportCategory(context);
        }

        public static async Task SeedSuspensionRole(AppDbContext context)
        {
            if (!await context.Roles.AnyAsync(x => x.Name == HelperConstants.SUSPENSION_ROLE))
            {
                Role role = new Role
                {
                    Name = HelperConstants.SUSPENSION_ROLE,
                    NormalizedName = HelperConstants.SUSPENSION_ROLE.ToUpper(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };

                context.Roles.Add(role);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// This method add super admin role to the database
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static async Task SeedSuperAdminRole(AppDbContext context)
        {
            if (!await context.Roles.AnyAsync(x => x.Name == HelperConstants.SUPER_ADMIN_ROLE))
            {
                var permissions = context.Permissions.AsQueryable();

                Role role = new Role
                {
                    Name = HelperConstants.SUPER_ADMIN_ROLE,
                    NormalizedName = HelperConstants.SUPER_ADMIN_ROLE.ToUpper(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                context.Roles.Add(role);

                List<RolePermission> rolePermissions = new();
                foreach (var permission in permissions)
                {
                    rolePermissions.Add(new RolePermission
                    {
                        PermissionId = permission.Id,
                        RoleId = role.Id
                    });
                }

                context.RolePermissions.AddRange(rolePermissions);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// This method creates a default super admin user
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userManager"></param>
        /// <returns></returns>
        private static async Task SeedSuperAdminUsers(AppDbContext context, UserManager<User> userManager)
        {
            var superAdmins = await userManager.GetUsersInRoleAsync(HelperConstants.SUPER_ADMIN_ROLE);

            if (superAdmins.Any())
            {
                return;
            }

            Guid? countryId = await context.Country.Where(x => x.TwoLetterCode == Nager.Country.Alpha2Code.NG.ToString())
                                                .Select(x => x.Id)
                                                .FirstOrDefaultAsync();

            if (countryId is null || !countryId.HasValue)
            {
                return;
            }

            string pin = "1234";
            string hashedPin = BCrypt.Net.BCrypt.HashPassword(pin);

            User user1 = new User
            {
                FirstName = "Default",
                LastName = "Admin",
                Email = "sss@gmail.com",
                UserName = "ss@gmail.com",
                PinHash = hashedPin,
                IsPinSet = true,
                PhoneNumber = "+2348160451288",
                CountryId = countryId
            };


            var result1 = await userManager.CreateAsync(user1, "123Pro456");
            var result2 = await userManager.CreateAsync(user2, "123Pro456");

            if (!result1.Succeeded)
            {
                throw new Exception(string.Join(".", result1.Errors.Select(x => x.Description)));
            }

            if (!result2.Succeeded)
            {
                throw new Exception(string.Join(".", result2.Errors.Select(x => x.Description)));
            }

            await userManager.AddToRoleAsync(user1, HelperConstants.SUPER_ADMIN_ROLE);
            await userManager.AddToRoleAsync(user2, HelperConstants.SUPER_ADMIN_ROLE);
        }

        /// <summary>
        /// This method adds country data to the database.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static async Task SeedCountries(AppDbContext context)
        {
            if (await context.Country.AnyAsync())
            {
                return;
            }

            var countriesInfo = new Nager.Country.CountryProvider().GetCountries();
            foreach (var countryInfo in countriesInfo)
            {
                context.Add(new Country
                {
                    Name = countryInfo.CommonName,
                    TwoLetterCode = countryInfo.Alpha2Code.ToString(),
                    ThreeLetterCode = countryInfo.Alpha3Code.ToString(),
                });
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// This method adds permissions for each domain of the super admin.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static async Task SeedPermissions(AppDbContext context)
        {
            if (!await context.Permissions.AnyAsync(x => x.Category == PermissionCategory.Dashboard))
            {
                context.Permissions.Add(new Permission
                {
                    Description = "View Dashboard",
                    Type = PermissionType.Read,
                    Category = PermissionCategory.Dashboard
                });
            }

            if (!await context.Permissions.AnyAsync(x => x.Category == PermissionCategory.Subscription))
            {
                context.Permissions.Add(new Permission
                {
                    Description = "View Subscriptions",
                    Type = PermissionType.Read,
                    Category = PermissionCategory.Subscription
                });
            }

            if (!await context.Permissions.AnyAsync(x => x.Category == PermissionCategory.Wallet))
            {
                context.Permissions.Add(new Permission
                {
                    Description = "View Wallet",
                    Type = PermissionType.Read,
                    Category = PermissionCategory.Wallet
                });
            }

            if (!await context.Permissions.AnyAsync(x => x.Category == PermissionCategory.UserManagement))
            {
                context.Permissions.AddRange(
                    new Permission
                    {
                        Description = "View Users",
                        Type = PermissionType.Read,
                        Category = PermissionCategory.UserManagement
                    },
                    new Permission
                    {
                        Description = "Modify Users",
                        Type = PermissionType.Write,
                        Category = PermissionCategory.UserManagement
                    });
            }

            if (!await context.Permissions.AnyAsync(x => x.Category == PermissionCategory.RoleManagement))
            {
                context.Permissions.Add(new Permission
                {
                    Description = "View Roles",
                    Type = PermissionType.Read,
                    Category = PermissionCategory.RoleManagement
                });
            }

            if (!await context.Permissions.AnyAsync(x => x.Category == PermissionCategory.ActivityLog))
            {
                context.Permissions.Add(new Permission
                {
                    Description = "View Activity Logs",
                    Type = PermissionType.Read,
                    Category = PermissionCategory.ActivityLog
                });
            }

            if (!await context.Permissions.AnyAsync(x => x.Category == PermissionCategory.Request))
            {
                context.Permissions.AddRange(
                    new Permission
                    {
                        Description = "View Requests",
                        Type = PermissionType.Read,
                        Category = PermissionCategory.Request
                    },
                    new Permission
                    {
                        Description = "Manage Requests",
                        Type = PermissionType.Write,
                        Category = PermissionCategory.Request
                    });
            }

            if (!await context.Permissions.AnyAsync(x => x.Category == PermissionCategory.Settings))
            {
                context.Permissions.AddRange(
                    new Permission
                    {
                        Description = "View Settings",
                        Type = PermissionType.Read,
                        Category = PermissionCategory.Settings
                    },
                    new Permission
                    {
                        Description = "Manage Settings",
                        Type = PermissionType.Write,
                        Category = PermissionCategory.Settings
                    });
            }

            if (!await context.Permissions.AnyAsync(x => x.Category == PermissionCategory.Notifications))
            {
                context.Permissions.AddRange(
                    new Permission
                    {
                        Description = "View Notifications",
                        Type = PermissionType.Read,
                        Category = PermissionCategory.Notifications
                    },
                    new Permission
                    {
                        Description = "Manage Notifications",
                        Type = PermissionType.Write,
                        Category = PermissionCategory.Notifications
                    });
            }

            if (!await context.Permissions.AnyAsync(x => x.Category == PermissionCategory.RuleEngine))
            {
                context.Permissions.AddRange(
                    new Permission
                    {
                        Description = "View Rule Engine",
                        Type = PermissionType.Read,
                        Category = PermissionCategory.RuleEngine
                    },
                    new Permission
                    {
                        Description = "Manage Rule Engine",
                        Type = PermissionType.Write,
                        Category = PermissionCategory.RuleEngine
                    });
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// This method adds a general rule for salary disbursement percentage charge
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static async Task SeedRuleEngineForSalaryDisbursementCharge(AppDbContext context)
        {
            if (!await context.SalaryDisbursementChargeSettings.AnyAsync(x => x.SettingType == SalaryDisbursementSettingType.General))
            {
                context.SalaryDisbursementChargeSettings.Add(new SalaryDisbursementChargeSetting
                {
                    ChargeDeductionPeriod = ChargeDeductionPeriod.OnPaymentDay,
                    SettingType = SalaryDisbursementSettingType.General,
                    Percentage = 12.25
                });

                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedPaysCompaniesForSalaryDisbursementCharge(AppDbContext context)
        {
            if (await context.CompanySalaryDisbursementChargeSettings.AnyAsync())
            {
                return;
            }

            var chargeSetting = await context.SalaryDisbursementChargeSettings
                                                                .FirstOrDefaultAsync(x => x.SettingType == SalaryDisbursementSettingType.General);
            if (chargeSetting == null)
            {
                return;
            }

            context.CompanySalaryDisbursementChargeSettings.AddRange(new List<CompanySalaryDisbursementChargeSetting>
            {
                new CompanySalaryDisbursementChargeSetting
                {
                    CompanyName = "Sparta",
                    IsNewRecord = true,
                    PaysCompanyId = Guid.NewGuid().ToString(),
                    SettingStatus = SalaryDisbursementSettingStatus.Default,
                    SalaryDisbursementChargeSettingId = chargeSetting.Id,
                    SalaryPaymentDate = DateTime.UtcNow,
                    NextChargeDate = DateTime.UtcNow,
                },
                new CompanySalaryDisbursementChargeSetting
                {
                    CompanyName = "SpartaTalent",
                    IsNewRecord = true,
                    PaysCompanyId = Guid.NewGuid().ToString(),
                    SettingStatus = SalaryDisbursementSettingStatus.Default,
                    SalaryDisbursementChargeSettingId = chargeSetting.Id,
                    SalaryPaymentDate = DateTime.UtcNow,
                    NextChargeDate = DateTime.UtcNow,
                }
            });
            await context.SaveChangesAsync();
        }

        private static async Task SeedPayrollVendorChargeDeductionBasis(AppDbContext context)
        {
            if (await context.PayrollVendorChargeDeductionBasis.AnyAsync())
            {
                return;
            }

            context.PayrollVendorChargeDeductionBasis.AddRange(new List<PayrollVendorChargeDeductionBasis>
            {
                new PayrollVendorChargeDeductionBasis
                {
                    Name = "On Tax Remittance"
                },
                new PayrollVendorChargeDeductionBasis
                {
                    Name = "Monthly"
                }
            });
            await context.SaveChangesAsync();
        }

        private static async Task SeedPayrollVendorPercentsgeCharge(AppDbContext context)
        {
            if (await context.PayrollVendorChargeSetings.AnyAsync())
            {
                return;
            }

            var chargeDeductionBasis = await context.PayrollVendorChargeDeductionBasis.FirstOrDefaultAsync();
            if (chargeDeductionBasis == null)
            {
                return;
            }

            context.PayrollVendorChargeSetings.Add(new PayrollVendorChargeSeting
            {
                ChargeDeductionBasisId = chargeDeductionBasis.Id,
                Percentage = 0,
            });
            await context.SaveChangesAsync();
        }

        private static async Task SeedAutoSavingSettings(AppDbContext context)
        {
            if (await context.AutoSavingSettings.AnyAsync())
            {
                return;
            }

            context.AutoSavingSettings.AddRange(new List<AutoSavingSetting>
            {
                new AutoSavingSetting
                {
                   CreatedAt = DateTime.Now,
                   ModifiedAt = DateTime.Now,
                   DurationInMonths = 3,
                   Description = "3 Months",
                   InterestPercentage = 0,
                },
                new AutoSavingSetting
                {
                   CreatedAt = DateTime.Now,
                   ModifiedAt = DateTime.Now,
                   DurationInMonths = 6,
                   Description = "6 Months",
                   InterestPercentage = 0,
                },
                new AutoSavingSetting
                {
                   CreatedAt = DateTime.Now,
                   ModifiedAt = DateTime.Now,
                   DurationInMonths = 9,
                   Description = "9 Months",
                   InterestPercentage = 0,
                }
            });
            await context.SaveChangesAsync();
        }

        private static async Task SeedProductUpdateCategory(AppDbContext context)
        {
            if (await context.ProductUpdateCategories.AnyAsync())
            {
                return;
            }

            context.ProductUpdateCategories.Add(new ProductUpdateCategory
            {
                Name = "Release Notes"
            });
            await context.SaveChangesAsync();
        }

        private static async Task SeedSupportCategory(AppDbContext context)
        {
            var categoriesToCreate = new List<string> { "Subscription", "Bug Report", "Feature Request", "IT Support", "Pays" };
            if (await context.TicketCategories.CountAsync() == categoriesToCreate.Count)
                return;

            var categories = await context.TicketCategories.ToListAsync();
            var ticketCategories = categoriesToCreate
                .Where(category => !categories.Exists(c => c.Name.ToLower().Equals(category.ToLower())))
                .Select(category => new TicketCategory() { Name = category })
                .ToList();

            if (ticketCategories.Count > 0)
            {
                await context.TicketCategories.AddRangeAsync(ticketCategories);
                await context.SaveChangesAsync();
            }

        }
    }
}
