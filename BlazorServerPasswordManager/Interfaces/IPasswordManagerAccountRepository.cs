using Microsoft.AspNetCore.Http;

namespace BlazorServerPasswordManager.Interfaces;

public interface IPasswordManagerAccountRepository<T>
{
    Task<IEnumerable<T>> GetAllAccountsAsync(string userId);
    Task<T?> CreateAsync(T model);
    Task<T?> UpdateAsync(T model);
    Task<T?> DeleteAsync(T model);
    Task<UploadStatus> UploadCsvAsync(IFormFile file, string userid);
}