using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Radial.Data.Entities;
using Radial.Models;
using Radial.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Radial.Services
{
    public interface IWorld
    {
        IEnumerable<CharacterBase> Characters { get; }
        ObjectStore<PlayerCharacter> CharacterBackups { get; }
        ObjectStore<Location> Locations { get; }
        IEnumerable<Npc> Npcs { get; }
        Location OfflineLocation { get; }
        IEnumerable<PlayerCharacter> PlayerCharacters { get; }
        Location PurgatoryLocation { get; }
        Location StartLocation { get; }

        Task Save();

        Task Load();
    }

    public class World : IWorld
    {
        public static readonly string PurgatoryZCoord = "Purgatory";
        public static readonly string OfflineZCoord = "Offline";

        private readonly IServiceProvider _serviceProvider;

        public World(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            Locations = new ObjectStore<Location>(nameof(Locations), _serviceProvider);
            CharacterBackups = new ObjectStore<PlayerCharacter>(nameof(CharacterBackups), _serviceProvider);
        }
        public IEnumerable<CharacterBase> Characters => Locations.All.SelectMany(x => x.Characters);

        public ObjectStore<PlayerCharacter> CharacterBackups { get; }

        public ObjectStore<Location> Locations { get; }

        public IEnumerable<Npc> Npcs => Characters.OfType<Npc>();

        public IEnumerable<PlayerCharacter> PlayerCharacters => Characters.OfType<PlayerCharacter>();

        public Location PurgatoryLocation => Locations.Get($"0,0,{PurgatoryZCoord}");

        public Location OfflineLocation => Locations.Get($"0,0,{OfflineZCoord}");

        public Location StartLocation => Locations.Get("0,0,0");

        public Location LocateCharacter(string characterName)
        {
            return Locations.Find(x => x.Characters.Exists(character => character.Name == characterName));
        }

        public async Task Load()
        {
            if (!Locations.All.Any())
            {
                var scope = _serviceProvider.CreateScope();
                var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                var locationsPath = Path.Combine(env.ContentRootPath, "Resources", "Locations.json");
                var locations = JsonSerializer.Deserialize<Dictionary<string, Location>>(await File.ReadAllTextAsync(locationsPath));
                foreach (var location in locations.Values)
                {
                    Locations.AddOrUpdate(location.XYZ, location);
                }
            }
        }

        public async Task Save()
        {
            await Locations.Save();
            await CharacterBackups.Save();
        }
    }
}
