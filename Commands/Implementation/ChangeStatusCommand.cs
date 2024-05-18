using SuperAdmin.Service.Commands.Contracts;
using SuperAdmin.Service.Database;
using SuperAdmin.Service.Database.Entities;
using SuperAdmin.Service.Database.Enums;

namespace SuperAdmin.Service.Commands.Implementation
{
    public class ChangeStatusCommand : ITicketCommand
    {
        private readonly TicketStatus _ticketStatus;

        public ChangeStatusCommand(TicketStatus status)
        {
            _ticketStatus = status;
        }

        public async Task<Ticket> Execute(AppDbContext _appDbContext, Ticket ticket, string performedBy)
        {
            var log = new TicketActionLog
            {
                ActionType = ActionType.CHANGE_STATUS,
                OldValue = ticket.Status.ToString(),
                NewValue = _ticketStatus.ToString(),
                Message = $"Status changed to: {_ticketStatus}",
                TicketId = ticket.Id,
                PerformedByUserId = performedBy
            };
            await _appDbContext.TicketActionLogs.AddAsync(log);
            ticket.Status = _ticketStatus;
            ticket.TicketActionLogs.Add(log);
            return ticket;
        }
    }
}

