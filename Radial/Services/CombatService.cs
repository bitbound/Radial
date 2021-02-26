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
    public interface ICombatService
    {
        void ApplyActionBonus(IClientConnection client, TimeSpan elapsed);
        void InitiateNpcAttackOnSight(IClientConnection clientConnection);
        void ExecuteNpcActions(Location location);
    }

    public class CombatService : ICombatService
    {
        public void ApplyActionBonus(IClientConnection client, TimeSpan elapsed)
        {
            var actionBonusChange = elapsed.TotalSeconds * .5;
            if (client.Character.ActionBonusIncreasing)
            {
                client.Character.ActionBonus = Math.Min(1, client.Character.ActionBonus + actionBonusChange);
                if (client.Character.ActionBonus == 1)
                {
                    client.Character.ActionBonusIncreasing = false;
                }
            }
            else
            {
                client.Character.ActionBonus = Math.Max(0, client.Character.ActionBonus - actionBonusChange);
                if (client.Character.ActionBonus == 0)
                {
                    client.Character.ActionBonusIncreasing = true;
                }
            }
        }

        public void ExecuteNpcActions(Location location)
        {
            foreach (var npc in location.Npcs)
            {
                if (npc.State == CharacterState.InCombat)
                {
                    var shouldAttack = Calculator.RollForBool(npc.ChargeCurrent / npc.ChargeMax);
                    var target = Calculator.GetRandom(location.Players);
                    // TODO: Attack.
                }
            }
        }

        public void InitiateNpcAttackOnSight(IClientConnection client)
        {
            var location = client.Location;

            var combatNpc = location.Npcs.FirstOrDefault(x =>
                 x.AggressionModel > AggressionModel.OnAttacked && x.State == CharacterState.Normal);

            if (combatNpc is not null && client.Character.State != CharacterState.Dead)
            {
                client.Character.State = CharacterState.InCombat;
                combatNpc.State = CharacterState.InCombat;
            }
        }
    }
}
