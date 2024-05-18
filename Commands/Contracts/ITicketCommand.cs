using SuperAdmin.Service.Database;
using SuperAdmin.Service.Database.Entities;

namespace SuperAdmin.Service.Commands.Contracts
{
    public interface ITicketCommand
	{
        Task<Ticket> Execute(AppDbContext _appDbContext, Ticket ticket, string performedBy);
    }
}