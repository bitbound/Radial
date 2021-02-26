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
        void SpawnNpcCombatEncounters(IClientConnection client);
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

        public void SpawnNpcCombatEncounters(IClientConnection client)
        {
            var location = client.Location;
            var character = client.Character;

            if (character.State == Enums.CharacterState.Normal &&
                !location.IsSafeArea &&
                DateTime.Now - character.LastCombatEncounter >= TimeSpan.FromSeconds(10) &&
                !location.Npcs.Where(x=>x.AggressionModel > Enums.AggressionModel.OnAttacked).Any())
            {
                var spawn = Calculator.RollForBool(.5);

                if (!spawn)
                {
                    return;
                }

                _npcService.SpawnNpcsForParty(client, location, Enums.AggressionModel.PlayerOnSight);

                _combatService.InitiateNpcAttackOnSight(client);
            }
        }
    }
}
