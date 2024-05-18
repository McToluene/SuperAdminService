using SuperAdmin.Service.Commands.Contracts;
using SuperAdmin.Service.Database;
using SuperAdmin.Service.Database.Entities;
using SuperAdmin.Service.Database.Enums;

namespace SuperAdmin.Service.Commands.Implementation
{
    public class AssignUserCommand : ITicketCommand
    {
        private readonly User _userToAssign;

        public AssignUserCommand(User user)
        {
            _userToAssign = user;
        }

        public async Task<Ticket> Execute(AppDbContext _appDbContext, Ticket ticket, string performedBy)
        {

            var log = new TicketActionLog
            {
                ActionType = ActionType.ASSIGN_USER,
                OldValue = ticket.AssignedUserId,
                NewValue = _userToAssign.Id,
                Message = $"Assigned to {_userToAssign.FirstName + " " + _userToAssign.LastName}",
                TicketId = ticket.Id,
                PerformedByUserId = performedBy,
            };
            await _appDbContext.TicketActionLogs.AddAsync(log);

            ticket.AssignedUserId = _userToAssign.Id;
            ticket.Status = TicketStatus.ASSIGNED;
            ticket.TicketActionLogs.Add(log);
            return ticket;
        }
    }
}

