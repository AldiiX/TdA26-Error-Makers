using Microsoft.EntityFrameworkCore;
using tda26.Server.Data;
using tda26.Server.Data.Models;

namespace tda26.Server.Repositories;





public class AccountRepository(
    AppDbContext db
) : IAccountRepository {
    public async Task<Account?> GetByIdAsync(Guid uuid, CancellationToken ct = default) {
        return await db.Accounts
            .FirstOrDefaultAsync(a => a.Uuid == uuid, ct);
    }
    
    public async Task<Account?> GetByUsernameAsync(string username, CancellationToken ct = default) {
        return await db.Accounts
            .FirstOrDefaultAsync(a => a.Username == username, ct);
    }

    public async Task<List<Account>> GetAllAsync(CancellationToken ct = default) {
        return await db.Accounts.ToListAsync(ct);
    }

    public async Task CreateAsync(Account account, CancellationToken ct = default) {
        db.Accounts.Add(account);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Account account, CancellationToken ct = default) {
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