using SuperAdmin.Service.Commands.Contracts;
using SuperAdmin.Service.Database;
using SuperAdmin.Service.Database.Entities;
using SuperAdmin.Service.Database.Enums;

namespace SuperAdmin.Service.Commands.Implementation
{
    public class SetPriorityCommand : ITicketCommand
    {
        private readonly Priority _priority;

        public SetPriorityCommand(Priority priority)
        {
            _priority = priority;
        }

        public async Task<Ticket> Execute(AppDbContext _appDbContext, Ticket ticket, string performedBy)
        {
            var log = new TicketActionLog
            {
                ActionType = ActionType.CHANGE_PRIORITY,
                OldValue = ticket.Priority.ToString(),
                NewValue = _priority.ToString(),
                Message = $"Priority changed to: {_priority}",
                TicketId = ticket.Id,
                PerformedByUserId = performedBy
            };
            await _appDbContext.TicketActionLogs.AddAsync(log);

            ticket.Priority = _priority;
            ticket.TicketActionLogs.Add(log);

            return ticket;
        }
    }
}

