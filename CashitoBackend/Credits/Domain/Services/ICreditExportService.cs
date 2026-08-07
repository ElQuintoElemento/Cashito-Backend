using System.Threading.Tasks;

namespace CashitoBackend.Credits.Domain.Services;

public interface ICreditExportService
{
    Task<byte[]> GeneratePdfAsync(int creditId, int userId);
    Task<byte[]> GeneratePdfPublicAsync(int creditId, string token);
    Task<byte[]> GenerateExcelAsync(int creditId, int userId);
    Task<byte[]> GenerateExcelPublicAsync(int creditId, string token);
}
