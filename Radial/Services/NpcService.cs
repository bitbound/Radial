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
    public interface INpcService
    {
        void SpawnNpcsForParty(IClientConnection clientConnection, Location location, AggressionModel? aggressionModel = null);
        Npc GenerateRandomNpc(AggressionModel? aggressionModel, Location location);
    }

    public class NpcService : INpcService
    {
        private readonly IClientManager _clientManager;

        public NpcService(IClientManager clientManager)
        {
            _clientManager = clientManager;
        }

        public void SpawnNpcsForParty(IClientConnection clientConnection, Location location, AggressionModel? aggressionModel = null)
        {
            var partyMembers = _clientManager.GetPartyMembers(clientConnection);
            var npcsToSpawn = new Random().Next(0, partyMembers.Count() + 1);

            for (var i = 0; i < npcsToSpawn; i++)
            {
                location.Characters.Add(GenerateRandomNpc(aggressionModel, location));
            }
        }

        public Npc GenerateRandomNpc(AggressionModel? aggressionModel, Location location)
        {
            // TODO: Get random NPCs.
            var distanceFromCenter = Calculator.GetDistanceBetween(0, 0, location.XCoord, location.YCoord);
            var npc =  new Npc()
            {
                Name = "A Wandering Entity",
                AggressionModel = aggressionModel.HasValue ?
                        aggressionModel.Value :
                        Calculator.RollForBool(.5) ? AggressionModel.PlayerOnSight : AggressionModel.OnAttacked,
                CoreEnergy = new Random().Next((int)(distanceFromCenter * .75), (int)distanceFromCenter),
                Type = CharacterType.NPC,
                Glint = new Random().Next((int)(distanceFromCenter * .5), (int)(distanceFromCenter * 1.5)),
            };

            ApplyRandomAttributes(npc);
            return npc;
        }

        private void ApplyRandomAttributes(Npc npc)
        {
        }
    }
}
