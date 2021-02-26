using Radial.Services.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Services
{
    public interface ICombatService
    {
        void ApplyActionBonus(IClientConnection client, TimeSpan elapsed);
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
                if (client.Character.ActionBonus == 1)
                {
                    client.Character.ActionBonusIncreasing = false;
                }
            }
        }
    }
}
