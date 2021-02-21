using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Radial.Data;
using Radial.Data.Entities;
using Radial.Enums;
using Radial.Services.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Radial.Services
{
    public interface IDataService
    {
        Task<CharacterInfo> GetPlayerCharacter(string userId);
        Task<Location> GetXyzLocation(string xyz);
        Task InitializePlayer(RadialUser user);

        Task<RadialUser> LoadUser(string username);

        Task ReloadEntity<T>(T entity);

        Task SaveEntity<T>(T entity);

        Task WriteLog(LogLevel logLevel, string category, EventId eventId, string state, Exception exception, List<string> scopeStack);
    }

    public class DataService : IDataService
    {
        private readonly ApplicationDbContext _dbContext;

        public DataService(ApplicationDbContext applicationDbContext)
        {
            _dbContext = applicationDbContext;
        }

        public async Task<CharacterInfo> GetPlayerCharacter(string userId)
        {
            return await _dbContext.Users
                  .Where(x => x.Id == userId)
                  .Select(x => x.Character)
                  .FirstOrDefaultAsync();
        }

        public async Task<Location> GetXyzLocation(string xyz)
        {
            var split = xyz.Split(",");
            var x = long.Parse(split[0].Trim());
            var y = long.Parse(split[1].Trim());
            var z = split[2].Trim();

            return await _dbContext.Locations
                .FirstOrDefaultAsync(loc =>
                    loc.XCoord == x &&
                    loc.YCoord == y &&
                    loc.ZCoord == z);
        }

        public async Task InitializePlayer(RadialUser user)
        {
            if (user.Character.Location is null)
            {
                user.Character.Location = await GetXyzLocation("0,0,0");
                user.Character.Location.Players.Add(user.Character);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<RadialUser> LoadUser(string username)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName == username);
        }

        public Task ReloadEntity<T>(T entity)
        {
            return _dbContext.Entry(entity).ReloadAsync();
        }

        public async Task SaveEntity<T>(T entity)
        {
            _dbContext.Entry(entity);
            await _dbContext.SaveChangesAsync();
        }


        public async Task WriteLog(LogLevel logLevel, string category, EventId eventId, string state, Exception exception, List<string> scopeStack)
        {
            try
            {
                // Prevent re-entrancy.
                if (eventId.Name?.Contains("EntityFrameworkCore") == true)
                {
                    return;
                }

                _dbContext.EventLogs.Add(new EventLogEntry()
                {
                    StackTrace = exception?.StackTrace,
                    LogLevel = logLevel,
                    Message = $"[{logLevel}] [{string.Join(" - ", scopeStack)} - {category}] | Message: {state} | Exception: {exception?.Message}",
                    TimeStamp = DateTimeOffset.Now
                });

                await _dbContext.SaveChangesAsync();
            }
            catch { }
        }

    }
}
