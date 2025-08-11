using AmazeCareAPI.Models.DTOs;
using System.Threading.Tasks;

namespace AmazeCareAPI.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateToken(TokenUser user);
    }
}
