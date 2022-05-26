using MiniBank.Core;
using MiniBank.Core.UnitOfWork;

namespace MiniBank.Data
{
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly DatabaseContext _context;

        public EfUnitOfWork(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken token)
        {
            return await _context.SaveChangesAsync(token);
        }
    }
}