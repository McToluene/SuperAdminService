using System.Data;
using System.Text;
using Common.Services.Interfaces;
using Common.Services.Utility;
using Configurations.Utility;
using Microsoft.EntityFrameworkCore;
using SuperAdmin.Service.Commands.Implementation;
using SuperAdmin.Service.Database;
using SuperAdmin.Service.Database.Entities;
using SuperAdmin.Service.Database.Enums;
using SuperAdmin.Service.Helpers;
using SuperAdmin.Service.Models.Dtos;
using SuperAdmin.Service.Models.Dtos.TicketDomain;
using SuperAdmin.Service.Models.Enums;
using SuperAdmin.Service.Services.Contracts;

namespace SuperAdmin.Service.Services.Implementations
{
    public class TicketService : ITicketService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IUserService _userService;
        private readonly ITicketCategoryService _ticketCategoryService;
        // private readonly IGCPBucketService _gcpBucketService;
        private readonly ITenantService _tenantService;

        public TicketService(AppDbContext appDbContext, ITicketCategoryService ticketCategoryService, IUserService userService, ITenantService tenantService)
        {
            _appDbContext = appDbContext;
            _userService = userService;
            _ticketCategoryService = ticketCategoryService;
            // _gcpBucketService = gcpBucketService;
            _tenantService = tenantService;
        }

        /// <summary>
        /// This method creates a ticket
        /// </summary>
        /// <param name="superAdminCreateTicket"></param>
        public async Task<ApiResponse<TicketDto>> CreateTicket(SuperAdminCreateTicket superAdminCreateTicket)
        {
            if (CheckExistingSubjectForCustomer(superAdminCreateTicket.Subject, superAdminCreateTicket.CustomerEmail))
            {
                return new ApiResponse<TicketDto>
                {
                    ResponseCode = "400",
                    ResponseMessage = $"Ticket with the subject '{superAdminCreateTicket.Subject}' already exists for customer!"
                };
            }

            if (!await _ticketCategoryService.IsExist(superAdminCreateTicket.CategoryId))
            {
                return new ApiResponse<TicketDto>
                {
                    ResponseCode = "404",
                    ResponseMessage = $"Ticket category '{superAdminCreateTicket.CategoryId}' not found!"
                };
            }

            User? user = await _userService.GetUser(superAdminCreateTicket.AssignedUserId);
            if (user == null)
            {
                return new ApiResponse<TicketDto>
                {
                    ResponseCode = "404",
                    ResponseMessage = $"User id '{superAdminCreateTicket.AssignedUserId}' not found!"
                };
            }

            Ticket ticket = new()
            {
                CustomerName = superAdminCreateTicket.CustomerName,
                CustomerEmail = superAdminCreateTicket.CustomerEmail,
                TicketCategoryId = superAdminCreateTicket.CategoryId,
                Priority = superAdminCreateTicket.Priority,
                Subject = superAdminCreateTicket.Subject,
                AssignedUserId = user.Id,
                Message = superAdminCreateTicket.Message,
                ReferenceId = UniqueIdentifierGenerator.GenerateUniqueTicketIdWithTimestamp(15)
            };

            await _appDbContext.AddAsync(ticket);
            await _appDbContext.SaveChangesAsync();
            return new ApiResponse<TicketDto>
            {
                Data = BuildTicketDto(ticket),
                ResponseCode = "200",
                ResponseMessage = $"Ticket with subject {ticket.Subject} created successfully"
            };
        }

        /// <summary>
        /// This method creates a ticket
        /// </summary>
        /// <param name="adminCreateTicket"></param>
        public async Task<ApiResponse<TicketDto>> CreateTicket(AdminCreateTicket adminCreateTicket, string name, string email)
        {
            if (!await _ticketCategoryService.IsExist(adminCreateTicket.CategoryId))
            {
                return new ApiResponse<TicketDto>
                {
                    ResponseCode = "404",
                    ResponseMessage = $"Ticket category '{adminCreateTicket.CategoryId}' not found!"
                };
            }

            Ticket ticket = new()
            {
                CustomerName = name,
                CustomerEmail = email,
                TicketCategoryId = adminCreateTicket.CategoryId,
                Subject = adminCreateTicket.Subject,
                Message = adminCreateTicket.Message,
                ReferenceId = UniqueIdentifierGenerator.GenerateUniqueTicketIdWithTimestamp(15)
            };

            await _appDbContext.AddAsync(ticket);
            await _appDbContext.SaveChangesAsync();
            return new ApiResponse<TicketDto>
            {
                Data = BuildTicketDto(ticket),
                ResponseCode = "200",
                ResponseMessage = $"Ticket with subject {ticket.Subject} created successfully"
            };
        }

        /// <summary>
        /// This method filters tickets
        /// </summary>
        /// <param name="filter"></param>
        public async Task<ApiResponse<PaginationResult<TicketDto>>> GetTickets(SortTicket filter)
        {
            var query = _appDbContext.Tickets
                .Where(t => !t.IsDeleted)
                .Include(t => t.TicketCategory)
                .Include(t => t.TicketReplies)
                .AsQueryable();

            query = ApplyIsReadFilter(query, filter.IsRead);
            query = ApplySearchKeyword(query, filter.SearchKeyword);
            query = ApplyStatusFilter(query, filter.TicketStatus);
            query = ApplyPriorityFilter(query, filter.Priority);
            query = ApplyCategoryFilter(query, filter.CategoryId);
            query = ApplySort(query, filter.SortBy, filter.OrderBy);


            var tickets = query.Select(t => BuildTicketDto(t));

            var data = await PaginationHelper.PaginateRecords(tickets, filter.Page!.Value, filter.PageSize!.Value);
            return new ApiResponse<PaginationResult<TicketDto>>
            {
                ResponseMessage = "Tickets fetched successfully!",
                Data = data,
                ResponseCode = "200",
            };
        }

        /// <summary>
        /// This method filters tickets
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="email"></param>
        public async Task<ApiResponse<PaginationResult<TicketDto>>> GetTickets(SortTicket filter, string? email)
        {
            var query = _appDbContext.Tickets
                .Where(t => t.CustomerEmail.Equals(email) && !t.IsDeleted)
                .Include(t => t.TicketCategory)
                .AsQueryable();

            query = ApplyIsReadFilter(query, filter.IsRead);
            query = ApplySearchKeyword(query, filter.SearchKeyword);
            query = ApplyStatusFilter(query, filter.TicketStatus);
            query = ApplyPriorityFilter(query, filter.Priority);
            query = ApplyCategoryFilter(query, filter.CategoryId);
            query = ApplySort(query, filter.SortBy, filter.OrderBy);

            var tickets = query.Select(t => BuildTicketDto(t));

            var data = await PaginationHelper.PaginateRecords(tickets, filter.Page!.Value, filter.PageSize!.Value);
            return new ApiResponse<PaginationResult<TicketDto>>
            {
                ResponseMessage = "Tickets fetched successfully!",
                Data = data,
                ResponseCode = "200",
            };
        }

        /// <summary>
        /// This method check a status filter and apply to query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="status"></param>
        private static IQueryable<Ticket> ApplyStatusFilter(IQueryable<Ticket> query, TicketStatus? status)
        {
            return status.HasValue
                ? query.Where(t => status.Equals(TicketStatus.UNRESOLVED) ? t.Status != status.Value : t.Status == status.Value)
                : query;
        }

        /// <summary>
        /// This method check a status filter and apply to query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="isRead"></param>
        private static IQueryable<Ticket> ApplyIsReadFilter(IQueryable<Ticket> query, bool? isRead)
        {
            return isRead.HasValue
                ? query.Where(t => isRead == t.IsRead)
                : query;
        }

        /// <summary>
        /// This method check a status filter and apply to query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="status"></param>
        private static IQueryable<Ticket> ApplyPriorityFilter(IQueryable<Ticket> query, Priority? priority)
        {
            return priority.HasValue
                ? query.Where(t => t.Priority == priority.Value)
                : query;
        }

        /// <summary>
        /// This method check a category filter and apply to query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="category"></param>
        private static IQueryable<Ticket> ApplyCategoryFilter(IQueryable<Ticket> query, Guid? category)
        {
            return category.HasValue
                ? query.Where(t => t.TicketCategoryId == category.Value)
                : query;
        }

        /// <summary>
        /// This method applies sorting to the query.
        /// </summary>
        /// <param name="query">The query to be sorted.</param>
        /// <param name="sortBy">The order by enum value.</param>
        /// <param name="orderBy">The order by enum value.</param>
        private static IQueryable<Ticket> ApplySort(IQueryable<Ticket> query, TicketSort? sortBy, OrderBy? orderBy)
        {
            return (orderBy, sortBy) switch
            {
                (null, null) => query.OrderByDescending(t => t.CreatedAt),
                (OrderBy.RecentlyAdded, null) => query.OrderByDescending(t => t.CreatedAt),
                (OrderBy.Oldest, null) => query.OrderBy(t => t.CreatedAt),
                (null, TicketSort.CreatedAt) => query.OrderByDescending(t => t.CreatedAt),
                (OrderBy.RecentlyAdded, TicketSort.CreatedAt) => query.OrderByDescending(t => t.CreatedAt),
                (OrderBy.Oldest, TicketSort.CreatedAt) => query.OrderBy(t => t.CreatedAt),
                (null, TicketSort.Status) => query.OrderByDescending(t => t.Status),
                (OrderBy.RecentlyAdded, TicketSort.Status) => query.OrderByDescending(t => t.Status),
                (OrderBy.Oldest, TicketSort.Status) => query.OrderBy(t => t.Status),
                (null, TicketSort.Category) => query.OrderByDescending(t => t.TicketCategory.Name),
                (OrderBy.RecentlyAdded, TicketSort.Category) => query.OrderByDescending(t => t.TicketCategory.Name),
                (OrderBy.Oldest, TicketSort.Category) => query.OrderBy(t => t.TicketCategory.Name),
                _ => throw new ArgumentException("Invalid combination of orderBy and sortBy parameters."),
            };
        }

        /// <summary>
        /// This method applies search to the query.
        /// </summary>
        /// <param name="query">The query to be search.</param>
        /// <param name="searchKeyword">The search keyword.</param>
        private static IQueryable<Ticket> ApplySearchKeyword(IQueryable<Ticket> query, string? searchKeyword)
        {
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                searchKeyword = searchKeyword.ToLower();

                query = query.Where(t =>
                    t.CustomerName.ToLower().Contains(searchKeyword) ||
                    t.CustomerEmail.ToLower().Contains(searchKeyword) ||
                    t.TicketCategory.Name.ToLower().Contains(searchKeyword) ||
                    t.Subject.ToLower().Contains(searchKeyword) ||
                    t.Message.ToLower().Contains(searchKeyword));
            }
            return query;
        }

        /// <summary>
        /// This method check an existing ticket category name
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="customerEmail"></param>
        private bool CheckExistingSubjectForCustomer(string subject, string customerEmail)
        {
            return _appDbContext.Tickets
                .Where(tc => tc.CustomerEmail.Equals(customerEmail.Trim()) && tc.Subject.ToLower().Equals(subject.Trim().ToLower()))
                .Any();
        }

        /// <summary>
        /// This method to assign a user to a ticket
        /// </summary>
        /// <param name="ticketId"></param>
        /// <param name="userToAssign"></param>
        public async Task<ApiResponse<TicketDto>> AssignUser(Guid ticketId, string userToAssign, string performedBy)
        {
            using var transaction = _appDbContext.Database.BeginTransaction();
            try
            {
                var ticket = await GetTicketWithDetails(ticketId);
                if (ticket == null)
                {
                    return new ApiResponse<TicketDto>
                    {
                        ResponseCode = "404",
                        ResponseMessage = $"Ticket id '{ticketId}' not found!"
                    };
                }

                User? user = await _userService.GetUser(userToAssign);
                if (user == null)
                {
                    return new ApiResponse<TicketDto>
                    {
                        ResponseCode = "404",
                        ResponseMessage = $"User id '{userToAssign}' not found!"
                    };
                }

                if (ticket.AssignedUserId != null && ticket.AssignedUserId.Equals(userToAssign))
                    return new ApiResponse<TicketDto>
                    {
                        ResponseCode = "409",
                        ResponseMessage = $"User is currently assigned to the ticket"
                    };

                ticket = await new AssignUserCommand(user).Execute(_appDbContext, ticket, performedBy);
                _appDbContext.Update(ticket);
                await _appDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                if (user.Email != null)
                {
                    User? assignedBy = await _userService.GetUser(performedBy);
                    if (assignedBy != null)
                        await SendAssignedMail(ticket, user, assignedBy);
                }

                return new ApiResponse<TicketDto>
                {
                    Data = BuildTicketDto(ticket),
                    ResponseCode = "200",
                    ResponseMessage = $"Ticket {ticket.Id} status updated successfully!"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<TicketDto>
                {
                    ResponseCode = "400",
                    ResponseMessage = $"Failed to assign user to ticket. {ex.Message}"
                };
            }
        }

        /// <summary>
        /// This method to assign a user to a ticket
        /// </summary>
        /// <param name="ticketId"></param>
        /// <param name="status"></param>
        /// <param name="performedBy"></param>
        public async Task<ApiResponse<TicketDto>> ChangeStatus(Guid ticketId, TicketStatus status, string performedBy)
        {
            using var transaction = _appDbContext.Database.BeginTransaction();
            try
            {
                var ticket = await GetTicketWithDetails(ticketId);
                if (ticket == null)
                {
                    return new ApiResponse<TicketDto>
                    {
                        ResponseCode = "404",
                        ResponseMessage = $"Ticket id '{ticketId}' not found!"
                    };
                }

                if (ticket.Status.Equals(status))
                {
                    return new ApiResponse<TicketDto>
                    {
                        ResponseCode = "409",
                        ResponseMessage = $"Status '{status}' is already assigned to the ticket"
                    };
                }

                ticket = await new ChangeStatusCommand(status).Execute(_appDbContext, ticket, performedBy);
                _appDbContext.Update(ticket);
                await _appDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                if (status.Equals(TicketStatus.RESOLVED))
                {
                    var actionPerformedBy = await _userService.GetUser(performedBy);
                    if (actionPerformedBy != null)
                        await SendResolvedMail(ticket, actionPerformedBy);
                }

                return new ApiResponse<TicketDto>
                {
                    Data = BuildTicketDto(ticket),
                    ResponseCode = "200",
                    ResponseMessage = $"Ticket {ticket.Id} status updated successfully!"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<TicketDto>
                {
                    ResponseCode = "400",
                    ResponseMessage = $"Failed to change ticket status. {ex.Message}"
                };
            }
        }

        /// <summary>
        /// This method to set a priority to a ticket
        /// </summary>
        /// <param name="ticketId"></param>
        /// <param name="priority"></param>
        /// <param name="performedBy"></param>
        public async Task<ApiResponse<TicketDto>> SetPriority(Guid ticketId, Priority priority, string performedBy)
        {
            using var transaction = _appDbContext.Database.BeginTransaction();
            try
            {
                var ticket = await GetTicketWithDetails(ticketId);
                if (ticket == null)
                {
                    return new ApiResponse<TicketDto>
                    {
                        ResponseCode = "404",
                        ResponseMessage = $"Ticket id '{ticketId}' not found!"
                    };
                }

                if (ticket.Priority.Equals(priority))
                {
                    return new ApiResponse<TicketDto>
                    {
                        ResponseCode = "409",
                        ResponseMessage = $"Priority '{priority}' is already assigned to the ticket"
                    };
                }

                ticket = await new SetPriorityCommand(priority).Execute(_appDbContext, ticket, performedBy);
                _appDbContext.Update(ticket);
                await _appDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ApiResponse<TicketDto>
                {
                    Data = BuildTicketDto(ticket),
                    ResponseCode = "200",
                    ResponseMessage = $"Ticket {ticket.Id} priority updated successfully!"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<TicketDto>
                {
                    ResponseCode = "400",
                    ResponseMessage = $"Failed to set priority to ticket. {ex.Message}"
                };
            }

        }

        /// <summary>
        /// This method perform all command on a ticket
        /// </summary>
        /// <param name="ticketId"></param>
        /// <param name="command"></param>
        /// <param name="performedBy"></param>
        public async Task<ApiResponse<TicketDto>> PerformAllAction(Guid ticketId, TicketCommand command, string performedBy)
        {
            using var transaction = _appDbContext.Database.BeginTransaction();
            try
            {
                var ticket = await GetTicketWithDetails(ticketId);
                if (ticket == null)
                {
                    return new ApiResponse<TicketDto>
                    {
                        ResponseCode = "404",
                        ResponseMessage = $"Ticket id '{ticketId}' not found!"
                    };
                }

                User? user = await _userService.GetUser(command.AssignUserId);
                if (user == null)
                {
                    return new ApiResponse<TicketDto>
                    {
                        ResponseCode = "404",
                        ResponseMessage = $"User id '{command.AssignUserId}' not found!"
                    };
                }

                ticket = await new SetPriorityCommand(command.Priority).Execute(_appDbContext, ticket, performedBy);
                ticket = await new ChangeStatusCommand(command.TicketStatus).Execute(_appDbContext, ticket, performedBy);
                ticket = await new AssignUserCommand(user).Execute(_appDbContext, ticket, performedBy);
                ticket = await new DueDateCommand(command.DueDate).Execute(_appDbContext, ticket, performedBy);

                _appDbContext.Update(ticket);
                await _appDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ApiResponse<TicketDto>
                {
                    Data = BuildTicketDto(ticket),
                    ResponseCode = "200",
                    ResponseMessage = $"Ticket {ticket.Id} updated successfully!"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<TicketDto>
                {
                    ResponseCode = "400",
                    ResponseMessage = $"Failed to perform action. {ex.Message}"
                };
            }
        }

        /// <summary>
        /// This method is to get a single ticket detail
        /// </summary>
        /// <param name="ticketId"></param>
        public async Task<ApiResponse<TicketDto>> GetTicket(Guid ticketId, bool isRead)
        {
            var ticket = await GetTicketWithDetails(ticketId);
            if (ticket == null)
            {
                return new ApiResponse<TicketDto>
                {
                    ResponseCode = "404",
                    ResponseMessage = $"Ticket id '{ticketId}' not found!"
                };
            }
            if (isRead && !ticket.IsRead)
            {
                ticket.IsRead = true;
                _appDbContext.Update(ticket);
                await _appDbContext.SaveChangesAsync();
            }
            return new ApiResponse<TicketDto>
            {
                Data = BuildTicketDto(ticket),
                ResponseCode = "200",
                ResponseMessage = $"Ticket '{ticket.Id}' fetched successfully!"
            };
        }

        /// <summary>
        /// This method is to transform ticket to ticketDto
        /// </summary>
        /// <param name="ticket"></param>
        private static TicketDto BuildTicketDto(Ticket ticket)
        {
            return new TicketDto
            {
                Id = ticket.Id,
                CustomerName = ticket.CustomerName,
                CustomerEmail = ticket.CustomerEmail,
                CategoryId = ticket.TicketCategoryId,
                AssignedUserId = ticket.AssignedUserId,
                Message = ticket.Message,
                Subject = ticket.Subject,
                Priority = ticket.Priority,
                CreatedAt = ticket.CreatedAt,
                Status = ticket.Status,
                DueDate = ticket.DueDate,
                ReferenceId = ticket.ReferenceId,
                IsRead = ticket.IsRead,
                TicketReplies = ticket.TicketReplies.Select(x => new TicketReplyDto() { Message = x.Message, MessageType = x.MessageType }).ToList(),
                CategoryName = ticket.TicketCategory?.Name,
                TicketActionLogs = ticket.TicketActionLogs
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => BuildTicketActivityLogDto(a))
                    .ToList()
            };
        }

        /// <summary>
        /// This method is to transform ticket to ticketDto
        /// </summary>
        /// <param name="ticket"></param>
        private static TicketActionLogDto? BuildTicketActivityLogDto(TicketActionLog? log)
        {
            return log == null ? null : new TicketActionLogDto
            {
                ActionType = log.ActionType,
                OldValue = log.OldValue,
                NewValue = log.NewValue,
                CreatedAt = log.CreatedAt,
                PerformedByUserId = log.PerformedByUserId,
                Message = log.Message
            };
        }

        /// <summary>
        /// This method is to get a single ticket detail
        /// </summary>
        /// <param name="id"></param>
        private async Task<Ticket?> GetTicketWithDetails(Guid id)
        {
            return await _appDbContext.Tickets
                .Where(t => t.Id.Equals(id) && !t.IsDeleted)
                .Include(t => t.TicketCategory)
                .Include(t => t.TicketActionLogs)
                .Include(t => t.AssignedUser)
                .Include(t => t.TicketReplies)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// This method is to get ticket statistics by status
        /// </summary>
        /// <param name="assignedUserId"></param>
        public async Task<ApiResponse<TicketStatistics>> GetTicketStatistics(string assignedUserId)
        {
            var ticketStatistics = await _appDbContext.Tickets
                .Where(ticket => !ticket.IsDeleted && string.IsNullOrEmpty(assignedUserId) || ticket.AssignedUserId == assignedUserId)
                .GroupBy(ticket => 1)
                .Select(group => new TicketStatistics
                {
                    TotalAssigned = group.Count(),
                    ResolvedCount = group.Count(ticket => ticket.Status == TicketStatus.RESOLVED),
                    AssignedCount = group.Count(ticket => ticket.Status == TicketStatus.ASSIGNED)
                })
                .FirstOrDefaultAsync();

            return new ApiResponse<TicketStatistics>
            {
                Data = ticketStatistics ?? new TicketStatistics(),
                ResponseCode = "200",
                ResponseMessage = $"Ticket statistics fetched successfully!"
            };
        }

        /// <summary>
        /// This method is to set due date to a ticket
        /// </summary>
        /// <param name="ticketId"></param>
        /// <param name="dateTime"></param>
        /// <param name="performedBy"></param>
        public async Task<ApiResponse<TicketDto>> SetDueDate(Guid ticketId, DateTime dateTime, string performedBy)
        {
            try
            {
                var ticket = await GetTicketWithDetails(ticketId);
                if (ticket == null)
                {
                    return new ApiResponse<TicketDto>
                    {
                        ResponseCode = "404",
                        ResponseMessage = $"Ticket id '{ticketId}' not found!"
                    };
                }

                if (ticket.DueDate.Equals(dateTime))
                {
                    return new ApiResponse<TicketDto>
                    {
                        ResponseCode = "409",
                        ResponseMessage = $"Due date '{dateTime}' is already assigned to the ticket"
                    };
                }


                ticket = await new DueDateCommand(dateTime).Execute(_appDbContext, ticket, performedBy);
                _appDbContext.Update(ticket);
                await _appDbContext.SaveChangesAsync();

                return new ApiResponse<TicketDto>
                {
                    Data = BuildTicketDto(ticket),
                    ResponseCode = "200",
                    ResponseMessage = $"Ticket '{ticket.Id}' due date set successfully!"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<TicketDto>
                {
                    ResponseCode = "400",
                    ResponseMessage = $"Failed to set due date to ticket. {ex.Message}"
                };
            }
        }

        /// <summary>
        /// This method assign multiple tickets to a user
        /// </summary>
        /// <param name="bulkTicket"></param>
        /// <param name="performedBy"></param>
        public async Task<ApiResponse<List<TicketDto>>> BulkAssign(BulkTicketAssign bulkTicket, string performedBy)
        {
            User? user = await _userService.GetUser(bulkTicket.UserToAssign);
            if (user == null)
            {
                return new ApiResponse<List<TicketDto>>
                {
                    ResponseCode = "404",
                    ResponseMessage = $"User id '{bulkTicket.UserToAssign}' not found!"
                };
            }

            var tickets = await _appDbContext.Tickets
                .Where(ticket => !ticket.IsDeleted && bulkTicket.Tickets.Contains(ticket.Id))
                .Include(t => t.TicketCategory)
                .Include(t => t.TicketActionLogs)
                .Include(t => t.AssignedUser)
                .ToListAsync();

            using var transaction = _appDbContext.Database.BeginTransaction();
            try
            {
                foreach (var ticket in tickets)
                {
                    var ticketToUpdate = ticket;
                    if (string.IsNullOrEmpty(ticket.AssignedUserId) || !ticket.AssignedUserId.Equals(bulkTicket.UserToAssign))
                        ticketToUpdate = await new AssignUserCommand(user).Execute(_appDbContext, ticketToUpdate, performedBy);

                    if (!ticket.Priority.Equals(bulkTicket.Priority))
                        ticketToUpdate = await new SetPriorityCommand(bulkTicket.Priority).Execute(_appDbContext, ticketToUpdate, performedBy);

                    if (!ticket.DueDate.Equals(bulkTicket.DueDate))
                        ticketToUpdate = await new DueDateCommand(bulkTicket.DueDate).Execute(_appDbContext, ticketToUpdate, performedBy);

                    _appDbContext.Update(ticketToUpdate);
                }

                await _appDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                if (user.Email != null)
                {
                    User? assignedBy = await _userService.GetUser(performedBy);
                    if (assignedBy != null)
                        await SendAssignedMail(tickets, user, assignedBy);
                }

                return new ApiResponse<List<TicketDto>>
                {
                    Data = tickets.Select(t => BuildTicketDto(t)).ToList(),
                    ResponseCode = "200",
                    ResponseMessage = $"Tickets assigned successfully!"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<TicketDto>>
                {
                    ResponseCode = "400",
                    ResponseMessage = $"Failed to perform action. {ex.Message}"
                };
            }
        }

        /// <summary>
        /// This method is to send a reply to a ticket.
        /// </summary>
        /// <param name="ticketId"></param>
        /// <param name="replyTicket"></param>
        /// <param name="performedBy"></param>
        public async Task<ApiResponse<List<ReplyDto>>> ReplyTicket(Guid ticketId, ReplyTicket replyTicket, string performedBy)
        {
            var ticket = await GetTicketWithDetails(ticketId);
            if (ticket == null)
            {
                return new ApiResponse<List<ReplyDto>>
                {
                    ResponseCode = "404",
                    ResponseMessage = $"Ticket id '{ticketId}' not found!"
                };
            }

            var message = string.Empty;
            if (replyTicket.MessageType.Equals(MessageType.MEDIA) && replyTicket.File != null)
            {
                return new ApiResponse<List<ReplyDto>>
                {
                    ResponseCode = "400",
                    ResponseMessage = $"Failed to upload file!"
                };
                // Guid guid = Guid.NewGuid();
                // var fileTrimmed = replyTicket.File.FileName.Replace(" ", "");
                // var fileName = guid.ToString().Replace('-', '0').Replace('_', '0').ToUpper() + "-" + fileTrimmed;
                // var res = await _gcpBucketService.UploadFileAsync(replyTicket.File, fileName);
                // if (res is not null)
                // {
                //     var url = await _gcpBucketService.GetSignedUrlAsync(fileName);
                //     message = url;
                // }
                // else
                //     return new ApiResponse<List<ReplyDto>>
                //     {
                //         ResponseCode = "400",
                //         ResponseMessage = $"Failed to upload file!"
                //     };
            }
            else
                message = replyTicket.Message ?? string.Empty;

            TicketReply ticketReply = new()
            {
                Message = message,
                PostedByUserId = performedBy,
                MessageType = replyTicket.MessageType,
                TicketId = ticketId,
                UserType = replyTicket.ReplyFrom
            };

            await _appDbContext.TicketReplies.AddAsync(ticketReply);
            await _appDbContext.SaveChangesAsync();

            await SendReplyMail(ticket, ticketReply);

            var replies = await _appDbContext.TicketReplies
                .Where(tr => tr.TicketId.Equals(ticketId))
                .OrderByDescending(tr => tr.CreatedAt)
                .Select(tr => new ReplyDto()
                {
                    Message = tr.Message,
                    MessageType = tr.MessageType,
                    PostedByUserId = tr.PostedByUserId,
                    UserType = tr.UserType
                })
                .ToListAsync();

            return new ApiResponse<List<ReplyDto>>
            {
                Data = replies,
                ResponseCode = "200",
                ResponseMessage = "Reply sent successfully!"
            };
        }

        private static async Task SendReplyMail(Ticket ticket, TicketReply ticketReply)
        {

            String emailBody = "$@"
                    + $"Dear {ticket.CustomerName}, <br/><br/>"
                    + "Thank you for reaching out to us. Here is the reply to your ticket: ";

            if (ticketReply.MessageType.Equals(MessageType.MEDIA))
                emailBody += $"<a href='" + ticketReply.Message + "'></a>";
            else
                emailBody += $" {ticketReply.Message}<br/>";

            emailBody += "<br/>Attached is any relevant file you might need.<br/>"
                    + "If you have any further questions or concerns, feel free to reply to this email.<br/><br/>"
                    + "Best regards,<br/>Pro";

            string subject = $"Ticket : {ticket.ReferenceId} reply.";
            await Utility.SendGridSendMail(emailBody, ticket.CustomerEmail, subject);
        }

        private static async Task SendAssignedMail(Ticket ticket, User assignedTo, User assignedBy)
        {
            string body = $@"
                <p>Hi {ticket.AssignedUser.FirstName}, </p>
                <p>I hope you're well. A new ticket has been assigned to you:</p>
                <ul>
                    <li><strong>Ticket ID:</strong> {ticket.ReferenceId}</li>
                    <li><strong>Description:</strong> {ticket.Message}</li>
                    <li><strong>Priority:</strong> {ticket.Priority}</li>
                    <li><strong>Due Date:</strong> {ticket.DueDate}</li>
                </ul>
                <a href = 'https://api.Pro.app/request' target = '_blank' >< button type = 'button' > Go to tickets</ button ></a> 
                < p>Please address this promptly. Reach out for any clarifications.</p>
                <p>Thanks,<br>
                    {assignedBy.FirstName + " " + assignedBy.LastName}<br>
                    Super Admin
                </p>";

            string subject = $"Ticket Assigned";
            await Utility.SendGridSendMail(body, assignedTo.Email, subject);
        }

        private static async Task SendAssignedMail(List<Ticket> tickets, User assignedTo, User assignedBy)
        {
            StringBuilder bodyBuilder = new();

            bodyBuilder.AppendLine($"<p>Hi {assignedTo.FirstName}, </p>");
            bodyBuilder.AppendLine("<p>I hope you're well. A new ticket has been assigned to you:</p>");

            foreach (var ticket in tickets)
            {
                bodyBuilder.AppendLine("<ul>");
                bodyBuilder.AppendLine($"<li><strong>Ticket ID:</strong> {ticket.ReferenceId}</li>");
                bodyBuilder.AppendLine($"<li><strong>Description:</strong> {ticket.Message}</li>");
                bodyBuilder.AppendLine($"<li><strong>Priority:</strong> {ticket.Priority}</li>");
                bodyBuilder.AppendLine($"<li><strong>Due Date:</strong> {ticket.DueDate}</li>");
                bodyBuilder.AppendLine("</ul>");
            }

            bodyBuilder.AppendLine($"<a href='https://api.Pro.app/request' target='_blank'>< button type = 'button'> Go to tickets </button ></a> ");
            bodyBuilder.AppendLine("<p>Please address this promptly. Reach out for any clarifications.</p>");
            bodyBuilder.AppendLine($"<p>Thanks,<br>{assignedBy.FirstName} {assignedBy.LastName}<br>Super Admin</p>");

            string body = bodyBuilder.ToString();

            string subject = $"Ticket Assigned";
            await Utility.SendGridSendMail(body, assignedTo?.Email, subject);
        }

        private static async Task SendResolvedMail(Ticket ticket, User resolvedBy)
        {
            string body = $@"
                <p>Dear {ticket.CustomerName},</p>
                <p>I'm pleased to inform you that the following ticket has been resolved:</p>
                <ul>
                    <li><strong>Ticket ID:</strong> {ticket.ReferenceId}</li>
                    <li><strong>Description:</strong> {ticket.Message}</li>
                    </ul>
                <p>Thank you for your attention to this matter. If you have any further questions or concerns, feel free to reach out.</p>
                <p>Best regards,<br>
                    {resolvedBy.FirstName + " " + resolvedBy.LastName}<br>
                </ p >
                ";

            string subject = $"Ticket Resolved";
            await Utility.SendGridSendMail(body, ticket.CustomerEmail, subject);
        }

        public async Task<ApiResponse<List<ReplyDto>>> GetReplies(Guid ticketId)
        {
            var ticket = await GetTicketWithDetails(ticketId);
            if (ticket == null)
            {
                return new ApiResponse<List<ReplyDto>>
                {
                    ResponseCode = "404",
                    ResponseMessage = $"Ticket id '{ticketId}' not found!"
                };
            }

            var replies = await _appDbContext.TicketReplies
                 .Where(tr => tr.TicketId.Equals(ticketId))
                 .Select(tr => new ReplyDto() { })
                 .ToListAsync();

            return new ApiResponse<List<ReplyDto>>
            {
                Data = replies,
                ResponseCode = "200",
                ResponseMessage = "Replies fetch successfully!"
            };
        }

        public async Task<ApiResponse<TicketDto>> DeleteTicket(Guid ticketId)
        {
            var ticket = await GetTicketWithDetails(ticketId);
            if (ticket == null)
            {
                return new ApiResponse<TicketDto>
                {
                    ResponseCode = "404",
                    ResponseMessage = $"Ticket id '{ticketId}' not found!"
                };
            }

            ticket.IsDeleted = true;
            _appDbContext.Tickets.Update(ticket);
            _appDbContext.SaveChanges();
            return new ApiResponse<TicketDto>
            {
                ResponseCode = "200",
                ResponseMessage = $"Ticket '{ticket.Id}' deleted successfully!"
            };
        }

        public async Task<ApiResponse<TicketDto>> DeleteTicket(Guid ticketId, string? email)
        {
            var ticket = await _appDbContext.Tickets
                .Where(t => t.Id.Equals(ticketId) && t.CustomerEmail.Equals(email))
                .FirstOrDefaultAsync();

            if (ticket == null)
            {
                return new ApiResponse<TicketDto>
                {
                    ResponseCode = "404",
                    ResponseMessage = $"Ticket id '{ticketId}' not found!"
                };
            }

            ticket.IsDeleted = true;
            _appDbContext.Tickets.Update(ticket);
            _appDbContext.SaveChanges();
            return new ApiResponse<TicketDto>
            {
                ResponseCode = "200",
                ResponseMessage = $"Ticket '{ticket.Id}' deleted successfully!"
            };
        }

        public async Task<int> TicketCountByCategory(string category, TicketStatus status)
        {
            return await _appDbContext.Tickets
                .Where(ticket => ticket.TicketCategoryId.ToString() == category && ticket.Status.Equals(status))
                .CountAsync();
        }
    }
}

