using Radial.Enums;
using Radial.Models;
using Radial.Services.Client;
using Radial.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Services
{
    public interface IEncounterService
    {
        void SpawnNpcs(IClientConnection client, TimeSpan sinceLastEncounter, double oddsOfEncounter, AggressionModel? aggressionModel = null);
    }
    public class EncounterService : IEncounterService
    {
        private readonly INpcService _npcService;
        private readonly ICombatService _combatService;

        public EncounterService(INpcService npcService, ICombatService combatService)
        {
            _npcService = npcService;
            _combatService = combatService;
        }


        public void SpawnNpcs(IClientConnection client, TimeSpan sinceLastEncounter, double oddsOfEncounter, AggressionModel? aggressionModel = null)
        {
            var location = client.Location;
            var character = client.Character;

            if (aggressionModel is null)
            {
                aggressionModel = Calculator.GetRandomEnum<AggressionModel>();
            }

            if (character.State == CharacterState.Normal &&
                !location.IsSafeArea &&
                DateTimeOffset.Now - character.LastCombatEncounter >= sinceLastEncounter &&
                !location.Npcs.Any(x => !x.IsFriendly))
            {
                var spawn = Calculator.RollForBool(oddsOfEncounter);

                if (!spawn)
                {
                    return;
                }

                var locationPlayerCount = location.PlayersAlive.Where(x => x.State != CharacterState.InCombat);
                var npcsToSpawn = Calculator.RandInstance.Next(0, locationPlayerCount.Count() + 2);

                for (var i = 0; i < npcsToSpawn; i++)
                {
                    location.AddCharacter(_npcService.GenerateRandomNpc(aggressionModel, location));
                }

                _combatService.InitiateNpcAttackOnSight(client);
            }
        }
    }
}
