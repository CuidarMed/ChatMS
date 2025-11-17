using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserCacheService
    {
        Task<UserDto> GetUserAsync(int userId);
        Task SetUserAsync(int userId, UserDto user);
        Task<Dictionary<int, UserDto>> GetUsersAsync(IEnumerable<int> userIds);
        Task SetUsersAsync(Dictionary<int, UserDto> users);
    }
}
