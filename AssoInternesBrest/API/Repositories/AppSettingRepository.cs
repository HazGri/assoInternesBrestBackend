using AssoInternesBrest.API.Data;
using AssoInternesBrest.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssoInternesBrest.API.Repositories
{
    public class AppSettingRepository(AppDbContext context) : IAppSettingRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<IEnumerable<AppSetting>> GetAllAsync()
        {
            return await _context.AppSettings.ToListAsync();
        }

        public async Task<AppSetting?> GetByKeyAsync(string key)
        {
            return await _context.AppSettings.FindAsync(key);
        }

        public async Task UpsertAsync(AppSetting setting)
        {
            AppSetting? existing = await _context.AppSettings.FindAsync(setting.Key);
            if (existing == null)
            {
                _context.AppSettings.Add(setting);
            }
            else
            {
                existing.Value = setting.Value;
                _context.AppSettings.Update(existing);
            }
            await _context.SaveChangesAsync();
        }
    }
}
