using Application.DTOs;
using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UserCacheService : IUserCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

        public UserCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<UserDto> GetUserAsync(int userId)
        {
            _cache.TryGetValue($"user_{userId}", out UserDto user);
            return Task.FromResult(user);
        }

        public Task SetUserAsync(int userId, UserDto user)
        {
            _cache.Set($"user_{userId}", user, _cacheExpiration);
            return Task.CompletedTask;
        }

        public Task<Dictionary<int, UserDto>> GetUsersAsync(IEnumerable<int> userIds)
        {
            var users = new Dictionary<int, UserDto>();
            foreach (var userId in userIds)
            {
                if (_cache.TryGetValue($"user_{userId}", out UserDto user))
                {
                    users[userId] = user;
                }
            }
            return Task.FromResult(users);
        }

        public Task SetUsersAsync(Dictionary<int, UserDto> users)
        {
            foreach (var kvp in users)
            {
                _cache.Set($"user_{kvp.Key}", kvp.Value, _cacheExpiration);
            }
            return Task.CompletedTask;
        }
    }
}
