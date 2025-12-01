using Microsoft.EntityFrameworkCore;
using tda26.Server.Data;
using tda26.Server.Data.Models;

namespace tda26.Server.Repositories;





public class AccountRepository(
    AppDbContext db
) : IAccountRepository {
    public async Task<Account?> GetByIdAsync(Guid uuid, CancellationToken ct = default) {
        var account = await db.Accounts
            .Include(a => a.Likes)
            .ThenInclude(l => l.Course)
            .FirstOrDefaultAsync(a => a.Uuid == uuid, ct);

        return account;
    }
    
    public async Task<Account?> GetByUsernameAsync(string username, CancellationToken ct = default) {
        var account = await db.Accounts
            .Include(a => a.Likes)
            .ThenInclude(l => l.Course)
            .FirstOrDefaultAsync(a => a.Username == username, ct);

        return account;
    }

    public async Task<List<Account>> GetAllAsync(CancellationToken ct = default) {
        var accounts = await db.Accounts
            .Include(a => a.Likes)
            .ThenInclude(l => l.Course)
            .ToListAsync(ct);

        return accounts;
    }

    public async Task CreateAsync(Account account, CancellationToken ct = default) {
        db.Accounts.Add(account);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Account account, CancellationToken ct = default) {
        account.UpdatedAt = DateTime.Now;
        
        db.Accounts.Update(account);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(Guid uuid, CancellationToken ct = default) {
        var account = await GetByIdAsync(uuid, ct);
        if (account == null) return false;
        
        db.Accounts.Remove(account);
        await db.SaveChangesAsync(ct);
            
        return true;

    }
}