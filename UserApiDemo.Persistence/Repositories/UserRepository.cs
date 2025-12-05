using Microsoft.EntityFrameworkCore;
using UserApiDemo.Domain.Entities;
using UserApiDemo.Domain.Interfaces;
using UserApiDemo.Persistence.Data;

namespace UserApiDemo.Persistence.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> FilterAsync(string? name, DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(u => u.FirstName.Contains(name) || u.LastName.Contains(name));
        }

        if (fromDate.HasValue)
        {
            var start = fromDate.Value.Date;
            query = query.Where(u => u.DateOfBirth.Date >= start);
        }

        if (toDate.HasValue)
        {
            var end = toDate.Value.Date;
            query = query.Where(u => u.DateOfBirth.Date <= end);
        }

        return await query.ToListAsync();
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _context.Users
            .AnyAsync(u => u.Username == username);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email);
    }
}
