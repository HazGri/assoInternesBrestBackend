using AssoInternesBrest.API.Entities;
using AssoInternesBrest.API.Repositories;

namespace AssoInternesBrest.API.Services
{
    public class AppSettingService(IAppSettingRepository repository) : IAppSettingService
    {
        private readonly IAppSettingRepository _repository = repository;

        public async Task<IEnumerable<AppSetting>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<string?> GetValueAsync(string key)
        {
            AppSetting? setting = await _repository.GetByKeyAsync(key);
            return setting?.Value;
        }

        public async Task SetValueAsync(string key, string value)
        {
            await _repository.UpsertAsync(new AppSetting { Key = key, Value = value });
        }
    }
}
