using Radial.Enums;
using Radial.Models;
using Radial.Utilities;

namespace Radial.Services
{
    public interface INpcService
    {
        Npc GenerateRandomNpc(AggressionModel? aggressionModel, Location location);
    }

    public class NpcService : INpcService
    {

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
                CorePower = Calculator.RandInstance.Next((int)(distanceFromCenter * .75), (int)distanceFromCenter + 1),
                Type = CharacterType.NPC
            };

            npc.Glint = (long)(Calculator.RandInstance.NextDouble() * npc.CorePower);
            npc.EnergyCurrent = npc.EnergyMax;
            npc.ChargeCurrent = (long)(Calculator.RandInstance.NextDouble() * npc.ChargeMax);

            return npc;
        }
    }
}
