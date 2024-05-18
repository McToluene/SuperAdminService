using Common.Services.Utility;
using Configurations.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SuperAdmin.Service.Database;
using SuperAdmin.Service.Database.Entities;
using SuperAdmin.Service.Extensions;
using SuperAdmin.Service.Helpers;
using SuperAdmin.Service.Models.Dtos;
using SuperAdmin.Service.Models.Dtos.UserDomain;
using SuperAdmin.Service.Models.Enums;
using SuperAdmin.Service.Services.Contracts;
using System.Data;

namespace SuperAdmin.Service.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IServiceProvider _serviceProvider;

        public UserService(
            AppDbContext appDbContext,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IServiceProvider serviceProvider)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// This method gets users
        /// </summary>
        /// <param name="filterBy"></param>
        /// <returns></returns>
        public async Task<ApiResponse<PaginationResult<UserDto>>> GetUsers(FilterUserBy filterBy)
        {
            var users = _userManager.Users.Where(x => !x.IsDeleted && !x.IsSuspended)
                                .Include(x => x.Country)
                                .Join(_appDbContext.UserRoles,
                                user => user.Id,
                                userRole => userRole.UserId,
                                (user, userRole) => new
                                {
                                    User = user,
                                    UserRole = userRole
                                })
                                .Join(_appDbContext.Roles,
                                firstJoin => firstJoin.UserRole.RoleId,
                                role => role.Id,
                                (firstJoin, role) => new UserDto
                                {
                                    FirstName = firstJoin.User.FirstName,
                                    LastName = firstJoin.User.LastName,
                                    Email = firstJoin.User.Email!,
                                    Phone = firstJoin.User.PhoneNumber!,
                                    CreatedAt = firstJoin.User.CreatedAt,
                                    Country = firstJoin.User.Country.Name,
                                    UserId = firstJoin.User.Id,
                                    Role = role.Name!,
                                    RoleId = role.Id
                                });

            users = FilterUsersByRole(filterBy.RoleName, users);
            users = OrderUsersBy(filterBy.OrderBy, users);

            var data = await PaginationHelper.PaginateRecords(users, filterBy.Page!.Value, filterBy.PageSize!.Value);
            MapDateToEpochTimestamp(data.Items);

            return new ApiResponse<PaginationResult<UserDto>>
            {
                Data = data,
                ResponseCode = "200",
            };
        }

        #region Get Deleted Users
        /// <summary>
        /// This endpoint gets deleted users
        /// </summary>
        /// <param name="filterBy"></param>
        /// <returns></returns>
        public async Task<ApiResponse<PaginationResult<UserDto>>> GetDeletedUsers(FilterUserBy filterBy)
        {
            var users = _userManager.Users.Where(x => x.IsDeleted)
                                .Include(x => x.Country)
                                .Join(_appDbContext.UserRoles,
                                user => user.Id,
                                userRole => userRole.UserId,
                                (user, userRole) => new
                                {
                                    User = user,
                                    UserRole = userRole
                                })
                                .Join(_appDbContext.Roles,
                                firstJoin => firstJoin.UserRole.RoleId,
                                role => role.Id,
                                (firstJoin, role) => new UserDto
                                {
                                    FirstName = firstJoin.User.FirstName,
                                    LastName = firstJoin.User.LastName,
                                    Email = firstJoin.User.Email!,
                                    Phone = firstJoin.User.PhoneNumber!,
                                    CreatedAt = firstJoin.User.CreatedAt,
                                    Country = firstJoin.User.Country.Name,
                                    UserId = firstJoin.User.Id,
                                    Role = role.Name!,
                                    RoleId = role.Id
                                });

            users = FilterUsersByRole(filterBy.RoleName, users);
            users = OrderUsersBy(filterBy.OrderBy, users);

            var data = await PaginationHelper.PaginateRecords(users, filterBy.Page!.Value, filterBy.PageSize!.Value);
            MapDateToEpochTimestamp(data.Items);

            return new ApiResponse<PaginationResult<UserDto>>
            {
                Data = data,
                ResponseCode = "200",
            };
        }
        #endregion

        #region Get Suspended Users
        /// <summary>
        /// This endpoint gets suspended users
        /// </summary>
        /// <param name="filterBy"></param>
        /// <returns></returns>
        public async Task<ApiResponse<PaginationResult<UserDto>>> GetSuspendedUsers(FilterUserBy filterBy)
        {
            var users = _userManager.Users.Where(x => !x.IsDeleted && x.IsSuspended)
                                .Include(x => x.Country)
                                .Join(_appDbContext.UserRoles,
                                user => user.Id,
                                userRole => userRole.UserId,
                                (user, userRole) => new
                                {
                                    User = user,
                                    UserRole = userRole
                                })
                                .Join(_appDbContext.Roles,
                                firstJoin => firstJoin.UserRole.RoleId,
                                role => role.Id,
                                (firstJoin, role) => new UserDto
                                {
                                    FirstName = firstJoin.User.FirstName,
                                    LastName = firstJoin.User.LastName,
                                    Email = firstJoin.User.Email!,
                                    Phone = firstJoin.User.PhoneNumber!,
                                    CreatedAt = firstJoin.User.CreatedAt,
                                    Country = firstJoin.User.Country.Name,
                                    UserId = firstJoin.User.Id,
                                    Role = role.Name!,
                                    RoleId = role.Id
                                })
                                .Where(x => x.Role != HelperConstants.SUSPENSION_ROLE)
                                .Select(x => new UserDto
                                {
                                    FirstName = x.FirstName,
                                    LastName = x.LastName,
                                    Email = x.Email,
                                    Phone = x.Phone,
                                    CreatedAt = x.CreatedAt,
                                    Country = x.Country,
                                    UserId = x.UserId,
                                    Role = x.Role,
                                    RoleId = x.RoleId
                                });

            users = FilterUsersByRole(filterBy.RoleName, users);
            users = OrderUsersBy(filterBy.OrderBy, users);

            var data = await PaginationHelper.PaginateRecords(users, filterBy.Page!.Value, filterBy.PageSize!.Value);
            MapDateToEpochTimestamp(data.Items);

            return new ApiResponse<PaginationResult<UserDto>>
            {
                Data = data,
                ResponseCode = "200",
            };
        }
        #endregion

        #region Create User
        /// <summary>
        /// This method creates a user, assigns a role and sends invitation to user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResponse<dynamic>> CreateUser(CreateUser model)
        {
            if (string.IsNullOrWhiteSpace(model.Email)
                || string.IsNullOrWhiteSpace(model.FirstName)
                || string.IsNullOrWhiteSpace(model.LastName)
                || string.IsNullOrWhiteSpace(model.PhoneNumber)
                || string.IsNullOrWhiteSpace(model.RoleName)
                || model.CountryId == Guid.Empty)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide required parameters"
                };
            }

            if (!model.PhoneNumber.StartsWith('+'))
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide valid phone number format"
                };
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is not null)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Email already exists"
                };
            }

            bool roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
            if (!roleExists)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "404",
                    ResponseMessage = "Role does not exist"
                };
            }

            var countryExists = await _appDbContext.Country.AnyAsync(x => x.Id == model.CountryId);
            if (!countryExists)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "404",
                    ResponseMessage = "Country does not exist"
                };
            }

            user = new User
            {
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Email = model.Email,
                UserName = model.Email,
                IsPinSet = false,
                PhoneNumber = model.PhoneNumber,
                CountryId = model.CountryId,
                HasPasswordChanged = false
            };
            string password = PasswordGenerator.GetRandomPassword(16);

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = string.Join(".", result.Errors.Select(x => x.Description))
                };
            }

            var identityResult = await _userManager.AddToRoleAsync(user, model.RoleName);
            if (!identityResult.Succeeded)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = string.Join(".", result.Errors.Select(x => x.Description))
                };
            }

            // Todo: Publish an event to send email which should be handled by the notification service
            bool isSuccessful = await SendEmailInviteNotification(model.Email, password, user.FirstName);
            if (!isSuccessful)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "200",
                    ResponseMessage = $"Account was created but failed to send email invitation to {model.Email}"
                };
            }

            return new ApiResponse<dynamic>
            {
                ResponseCode = "200",
                ResponseMessage = $"An invitation has been sent to {model.Email}"
            };
        }
        #endregion

        #region Update User
        public async Task<ApiResponse<dynamic>> UpdateUser(string userId, UpdateUser model)
        {
            if (string.IsNullOrWhiteSpace(model.PhoneNumber)
                && string.IsNullOrWhiteSpace(model.RoleName)
                && !model.CountryId.HasValue)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide a parameter to update"
                };
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null || user.IsDeleted)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "404",
                    ResponseMessage = "User does not exist"
                };
            }

            if (user.IsSuspended)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Cannot update a suspended user"
                };
            }

            (bool isSuccessful, string? message, user) = UpdateUserPhoneNumber(model.PhoneNumber, user);
            if (!isSuccessful)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = message
                };
            }

            (isSuccessful, message, user) = await UpdateUserRole(model.RoleName, user);
            if (!isSuccessful)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = message
                };
            }

            (isSuccessful, message, user) = await UpdateUserCountry(model.CountryId, user);
            if (!isSuccessful)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = message
                };
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = string.Join(".", result.Errors.Select(x => x.Description))
                };
            }

            return new ApiResponse<dynamic>
            {
                ResponseCode = "200",
                ResponseMessage = "User updated successfully"
            };
        }

        #endregion

        #region Suspend Users
        /// <summary>
        /// This method suspends users
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResponse<dynamic>> SuspendUsers(SuspendUser model)
        {
            if (model is null || !model.UserIds.Any())
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide required parameters"
                };
            }

            var users = await _userManager.Users.Where(x => model.UserIds.Contains(x.Id) && !x.IsDeleted).ToListAsync();
            if (!users.Any())
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "404",
                    ResponseMessage = "Users not found"
                };
            }

            foreach (var user in users)
            {
                bool isUserInSuspensionRole = await _userManager.IsInRoleAsync(user, HelperConstants.SUSPENSION_ROLE);
                if (!isUserInSuspensionRole)
                {
                    await _userManager.AddToRoleAsync(user, HelperConstants.SUSPENSION_ROLE);
                }

                user.IsSuspended = true;
                await _userManager.UpdateAsync(user);
            }

            return new ApiResponse<dynamic>
            {
                ResponseCode = "200",
                ResponseMessage = "Suspended users successfully"
            };
        }
        #endregion

        #region Reactivate Suspended Users
        /// <summary>
        /// This method reactivates suspended users
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResponse<dynamic>> ReactivateSuspendedUsers(ReactivateUser model)
        {
            if (model is null || !model.UserIds.Any())
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide required parameters"
                };
            }

            var suspendedUsers = await _userManager.Users.Where(x => model.UserIds.Contains(x.Id) && x.IsSuspended && !x.IsDeleted).ToListAsync();
            if (!suspendedUsers.Any())
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "404",
                    ResponseMessage = "Suspended users not found"
                };
            }

            foreach (var user in suspendedUsers)
            {
                bool isUserInSuspensionRole = await _userManager.IsInRoleAsync(user, HelperConstants.SUSPENSION_ROLE);
                if (isUserInSuspensionRole)
                {
                    await _userManager.RemoveFromRoleAsync(user, HelperConstants.SUSPENSION_ROLE);
                }

                user.IsSuspended = false;
                await _userManager.UpdateAsync(user);
            }

            return new ApiResponse<dynamic>
            {
                ResponseCode = "200",
                ResponseMessage = "Reactivated users successfully"
            };
        }
        #endregion

        #region Reset User Password
        public async Task<ApiResponse<dynamic>> ResetUserPassword(string userId, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide password"
                };
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null || user.IsDeleted)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "404",
                    ResponseMessage = "User does not exist"
                };
            }

            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, password);
            if (!result.Succeeded)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = string.Join(".", result.Errors.Select(x => x.Description))
                };
            }

            bool isSuccessful = await SendNewLoginCredentialNotification(user.Email!, password, user.FirstName);
            if (!isSuccessful)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Reset password succcessful but unable to send new login credential to user"
                };
            }

            return new ApiResponse<dynamic>
            {
                ResponseCode = "200",
                ResponseMessage = "Reset password succcessful"
            };
        }
        #endregion
        /// <summary>
        /// This method deletes a user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        #region Delete User 
        public async Task<ApiResponse<dynamic>> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null || user.IsDeleted)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "404",
                    ResponseMessage = "User does not exist"
                };
            }

            user.IsDeleted = true;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = string.Join(".", result.Errors.Select(x => x.Description))
                };
            }

            return new ApiResponse<dynamic>
            {
                ResponseCode = "200",
                ResponseMessage = "Deleted user successfully"
            };
        }
        #endregion

        /// <summary>
        /// This method get a user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        #region
        public async Task<User?> GetUser(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        #endregion

        #region Private Methods 
        /// <summary>
        /// This private method filters users by role name
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="users"></param>
        /// <returns></returns>
        private IQueryable<UserDto> FilterUsersByRole(string? roleName, IQueryable<UserDto> users)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return users;
            }

            return users.Where(x => x.Role == roleName);
        }

        /// <summary>
        /// This private method orders user records
        /// </summary>
        /// <param name="orderUserBy"></param>
        /// <param name="users"></param>
        /// <returns></returns>
        private IQueryable<UserDto> OrderUsersBy(OrderBy? orderUserBy, IQueryable<UserDto> users)
        {
            if (!orderUserBy.HasValue)
            {
                return users.OrderByDescending(x => x.CreatedAt).AsQueryable();
            }

            switch (orderUserBy.Value)
            {
                case OrderBy.RecentlyAdded:
                    users = users.OrderByDescending(x => x.CreatedAt).AsQueryable();
                    break;

                case OrderBy.Oldest:
                    users = users.OrderBy(x => x.CreatedAt).AsQueryable();
                    break;
            }

            return users;
        }

        /// <summary>
        /// This private method converts date time format to epoch timestamp format
        /// </summary>
        /// <param name="items"></param>
        private void MapDateToEpochTimestamp(UserDto[] items)
        {
            foreach (var item in items)
            {
                item.DateCreatedInEpochMilliseconds = item.CreatedAt.ToEpochTimestampInMilliseconds();
            }
        }

        /// <summary>
        /// This method sends email invite notification
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="firstName"></param>
        /// <returns></returns>
        private async Task<bool> SendEmailInviteNotification(string email, string password, string firstName)
        {
            var scope = _serviceProvider.CreateScope();
            var hostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

            string loginUrl = "";
            if (hostEnvironment.IsProduction())
            {
                loginUrl = "https://app.job/auth/login";
            }
            else
            {
                loginUrl = "https://sua.app/auth/login";
            }

            string body = $"Hello {firstName},<br/><br/>An account has been created for you by an admin. Use the details below to <a href=\"{loginUrl}\" target=\"_blank\">login</a> and complete your onboarding process.<br/><br/>Email: {email}<br/>Password: {password}<br/><br/>Regards,<br/>Super Admin<br/>Job";
            string subject = "Super Admin Invite";

            bool isSuccessful = await Utility.SendGridSendMail(body, email, subject);
            return isSuccessful;
        }

        /// <summary>
        /// This method sends new login credentials via email
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="firstName"></param>
        /// <returns></returns>
        private async Task<bool> SendNewLoginCredentialNotification(string email, string password, string firstName)
        {
            var scope = _serviceProvider.CreateScope();
            var hostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

            string loginUrl = "";
            if (hostEnvironment.IsProduction())
            {
                loginUrl = "https://login.pro/auth/login";
            }
            else
            {
                loginUrl = "https://login.app/auth/login";
            }

            string body = $"Hello {firstName},<br/><br/>Your password was reset by an admin. Use the details below to <a href=\"{loginUrl}\" target=\"_blank\">login</a> to the dashboard.<br/><br/>Email: {email}<br/>Password: {password}<br/><br/>Regards,<br/>Super Admin<br/>Job";
            string subject = "New Login Credential";

            bool isSuccessful = await Utility.SendGridSendMail(body, email, subject);
            return isSuccessful;
        }

        /// <summary>
        /// This private method updates user's role
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task<(bool, string?, User)> UpdateUserRole(string? roleName, User user)
        {
            if (!string.IsNullOrWhiteSpace(roleName))
            {
                string role = roleName.Trim();
                bool roleExists = await _roleManager.RoleExistsAsync(role);
                if (!roleExists)
                {
                    return (false, "Role does not exist", user);
                }

                bool isUserInRole = await _userManager.IsInRoleAsync(user, role);
                if (isUserInRole)
                {
                    return (true, null, user);
                }

                List<IdentityResult> results = new();
                var userRoles = await _userManager.GetRolesAsync(user);

                foreach (var userRole in userRoles)
                {
                    results.Add(await _userManager.RemoveFromRoleAsync(user, userRole));
                }

                if (results.Any(x => !x.Succeeded))
                {
                    var result = results.Where(x => !x.Succeeded).FirstOrDefault();
                    string errorMessage = string.Join(".", result!.Errors.Select(x => x.Description));
                    return (false, errorMessage, user);
                }

                var identityResult = await _userManager.AddToRoleAsync(user, role);
                if (!identityResult.Succeeded)
                {
                    string errMessage = string.Join(".", identityResult.Errors.Select(x => x.Description));
                    return (false, errMessage, user);
                }
            }

            return (true, null, user);
        }

        /// <summary>
        /// This method updates user's phone number
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private (bool, string?, User) UpdateUserPhoneNumber(string? phoneNumber, User user)
        {
            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                if (!phoneNumber.StartsWith('+'))
                {
                    return (false, "Please provide valid phone number format", user);
                }

                user.PhoneNumber = phoneNumber.Trim();
            }

            return (true, null, user);
        }

        /// <summary>
        /// This method updates user's country
        /// </summary>
        /// <param name="countryId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task<(bool, string?, User)> UpdateUserCountry(Guid? countryId, User user)
        {
            if (countryId.HasValue)
            {
                bool countryExists = await _appDbContext.Country.AnyAsync(x => x.Id == countryId.Value);
                if (!countryExists)
                {
                    return (false, "Country does not exist", user);
                }

                user.CountryId = countryId.Value;
            }

            return (true, null, user);
        }
        #endregion

    }
}
