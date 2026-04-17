using AssoInternesBrest.API.Entities;

namespace AssoInternesBrest.API.Services
{
    public interface IAppSettingService
    {
        Task<IEnumerable<AppSetting>> GetAllAsync();
        Task<string?> GetValueAsync(string key);
        Task SetValueAsync(string key, string value);
    }
}
