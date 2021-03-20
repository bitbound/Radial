using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
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
        ObjectStore<PlayerCharacter> CharacterBackups { get; }
        IEnumerable<CharacterBase> Characters { get; }
        ObjectStore<Interactable> Interactables { get; }
        ObjectStore<Location> Locations { get; }
        IEnumerable<Npc> Npcs { get; }
        IEnumerable<PlayerCharacter> PlayerCharacters { get; }
        Location StartLocation { get; }

        Task Load();
        Task Save();
    }

    public class World : IWorld
    {

        private readonly IServiceProvider _serviceProvider;

        public World(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            Locations = new ObjectStore<Location>(nameof(Locations), _serviceProvider);
            CharacterBackups = new ObjectStore<PlayerCharacter>(nameof(CharacterBackups), _serviceProvider);
            Interactables = new ObjectStore<Interactable>(nameof(Interactables), _serviceProvider);
        }
        public ObjectStore<PlayerCharacter> CharacterBackups { get; }
        public IEnumerable<CharacterBase> Characters => Locations.All.SelectMany(x => x.Characters);
        public ObjectStore<Interactable> Interactables { get; }
        public ObjectStore<Location> Locations { get; }

        public IEnumerable<Npc> Npcs => Characters.OfType<Npc>();

        public IEnumerable<PlayerCharacter> PlayerCharacters => Characters.OfType<PlayerCharacter>();

        public Location StartLocation => Locations.Get("0,0,0");
        public async Task Load()
        {
            var scope = _serviceProvider.CreateScope();
            var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            var locationsPath = Path.Combine(env.ContentRootPath, "Resources", "Locations.json");
            var locations = JsonSerializer.Deserialize<Dictionary<string, Location>>(await File.ReadAllTextAsync(locationsPath));
            foreach (var location in locations.Values)
            {
                if (!Locations.Exists(location.XYZ))
                {
                    Locations.AddOrUpdate(location.XYZ, location);
                }
            }

            foreach (var location in Locations.All)
            {
                if (location.IsTemporary)
                {
                    Locations.Remove(location.XYZ, out _);
                    continue;
                }
                foreach (var player in location.Players)
                {
                    Locations.Get(location.XYZ).Players.Remove(player);
                }
            }
        }

        public Location LocateCharacter(string characterName)
        {
            return Locations.Find(x => x.Characters.Any(character => character.Name == characterName));
        }
        public async Task Save()
        {
            await Locations.Save();
            await CharacterBackups.Save();
            await Interactables.Save();
        }
    }
}
