using AssoInternesBrest.API.Entities;

namespace AssoInternesBrest.API.Repositories
{
    public interface IAppSettingRepository
    {
        Task<IEnumerable<AppSetting>> GetAllAsync();
        Task<AppSetting?> GetByKeyAsync(string key);
        Task UpsertAsync(AppSetting setting);
    }
}
