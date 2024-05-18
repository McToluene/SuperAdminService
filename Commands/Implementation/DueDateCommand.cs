using SuperAdmin.Service.Commands.Contracts;
using SuperAdmin.Service.Database;
using SuperAdmin.Service.Database.Entities;
using SuperAdmin.Service.Database.Enums;

namespace SuperAdmin.Service.Commands.Implementation
{
    public class DueDateCommand : ITicketCommand
    {
        private readonly DateTime _date;

        public DueDateCommand(DateTime dateTime)
        {
            _date = dateTime;
        }

        public async Task<Ticket> Execute(AppDbContext _appDbContext, Ticket ticket, string performedBy)
        {
            var log = new TicketActionLog
            {
                ActionType = ActionType.ASSIGN_DUEDATE,
                OldValue = ticket.DueDate.ToString(),
                NewValue = _date.ToString(),
                Message = $"Due date changed to: {_date}",
                TicketId = ticket.Id,
                PerformedByUserId = performedBy
            };

            ticket.DueDate = _date;
            ticket.TicketActionLogs.Add(log);

            await _appDbContext.TicketActionLogs.AddAsync(log);
            return ticket;
        }
    }
}

