using Radial.Data.Entities;
using Radial.Models;
using Radial.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Services
{
    public interface IWorld
    {
        IEnumerable<Character> Characters { get; }
        ObjectStore<Location> Locations { get; }
        IEnumerable<Npc> Npcs { get; }
        IEnumerable<PlayerCharacter> PlayerCharacters { get; }
        Task Save();
    }

    public class World : IWorld
    {
        private readonly IServiceProvider _serviceProvider;

        public World(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            Locations = new ObjectStore<Location>(nameof(Locations), _serviceProvider);
        }
        public ObjectStore<Location> Locations { get; }

        public IEnumerable<Character> Characters => Locations.All.SelectMany(x => x.Characters);

        public IEnumerable<Npc> Npcs => Characters.OfType<Npc>();

        public IEnumerable<PlayerCharacter> PlayerCharacters => Characters.OfType<PlayerCharacter>();

        public Location LocateCharacter(Guid characterId)
        {
            return Locations.All.FirstOrDefault(x => x.Characters.Exists(character => character.Id == characterId));
        }

        public async Task Save()
        {
            await Locations.Save();
        }
    }
}
