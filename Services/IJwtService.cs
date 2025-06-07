using EmployeeScheduling.API.Models;

namespace EmployeeScheduling.API.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        bool ValidateToken(string token);
    }
}
