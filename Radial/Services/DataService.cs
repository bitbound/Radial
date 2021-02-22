using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
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

        Task<RadialUser> LoadUser(string username);

        Task SaveEntity<T>(T entity);

        Task WriteLog(LogLevel logLevel, string category, EventId eventId, string state, Exception exception, List<string> scopeStack);
        Task<Location> LoadLocation(Guid locationId);
    }

    public class DataService : IDataService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public DataService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbFactory = dbContextFactory;
        }

        public async Task<CharacterInfo> GetPlayerCharacter(string userId)
        {
            using var dbContext = _dbFactory.CreateDbContext();
            return await dbContext.Users
                  .Where(x => x.Id == userId)
                  .Select(x => x.Character)
                  .FirstOrDefaultAsync();
        }

        public async Task<Location> GetXyzLocation(string xyz)
        {
            using var dbContext = _dbFactory.CreateDbContext();

            var split = xyz.Split(",");
            var x = long.Parse(split[0].Trim());
            var y = long.Parse(split[1].Trim());
            var z = split[2].Trim();

            return await dbContext.Locations
                .FirstOrDefaultAsync(loc =>
                    loc.XCoord == x &&
                    loc.YCoord == y &&
                    loc.ZCoord == z);
        }

        public Task<Location> LoadLocation(Guid locationId)
        {
            using var dbContext = _dbFactory.CreateDbContext();

            return dbContext.Locations
                .Include(x => x.Characters)
                .Include(x => x.Interactables)
                .FirstOrDefaultAsync(x => x.Id == locationId);
        }

        public async Task<RadialUser> LoadUser(string username)
        {
            using var dbContext = _dbFactory.CreateDbContext();

            var user = await dbContext.Users
                .Include(x => x.Character).ThenInclude(x => x.Location)
                .Include(x => x.Character).ThenInclude(x => x.Effects)
                .FirstOrDefaultAsync(x => x.UserName == username);

            if (user.Character.Location is null)
            {
                user.Character.Location = await GetXyzLocation("0,0,0");
                user.Character.Location.Players.Add(user.Character);
            }

            await dbContext.SaveChangesAsync();

            return user;
        }

        public async Task SaveEntity<T>(T entity)
        {
            using var dbContext = _dbFactory.CreateDbContext();

            dbContext.Update(entity);
            await dbContext.SaveChangesAsync();
        }


        public async Task WriteLog(LogLevel logLevel, string category, EventId eventId, string state, Exception exception, List<string> scopeStack)
        {
            try
            {
                using var dbContext = _dbFactory.CreateDbContext();

                // Prevent re-entrancy.
                if (eventId.Name?.Contains("EntityFrameworkCore") == true)
                {
                    return;
                }

                dbContext.EventLogs.Add(new EventLogEntry()
                {
                    StackTrace = exception?.StackTrace,
                    LogLevel = logLevel,
                    Message = $"[{logLevel}] [{string.Join(" - ", scopeStack)} - {category}] | Message: {state} | Exception: {exception?.Message}",
                    TimeStamp = DateTimeOffset.Now
                });

                await dbContext.SaveChangesAsync();
            }
            catch { }
        }

    }
}
