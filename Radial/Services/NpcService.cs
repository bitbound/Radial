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
            if (location.IsSafeArea)
            {
                return;
            }

            var locationPlayerCount = location.PlayersAlive.Where(x => x.State != CharacterState.InCombat);
            var npcsToSpawn = Calculator.RandInstance.Next(0, locationPlayerCount.Count() + 2);

            for (var i = 0; i < npcsToSpawn; i++)
            {
                location.AddCharacter(GenerateRandomNpc(aggressionModel, location));
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
                CoreEnergy = Calculator.RandInstance.Next((int)(distanceFromCenter * .75), (int)distanceFromCenter + 1),
                Type = CharacterType.NPC
            };

            npc.Glint = (long)(Calculator.RandInstance.NextDouble() * npc.CoreEnergy);
            npc.EnergyCurrent = npc.EnergyMax;
            npc.ChargeCurrent = (long)(Calculator.RandInstance.NextDouble() * npc.ChargeMax);

            return npc;
        }
    }
}
