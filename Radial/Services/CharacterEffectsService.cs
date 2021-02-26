using Radial.Services.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Services
{
    public interface ICharacterEffectsService
    {
        void ApplyChargeRecovery(IClientConnection client, double timeMultiplier);
    }
    public class CharacterEffectsService : ICharacterEffectsService
    {
        public void ApplyChargeRecovery(IClientConnection client, double timeMultiplier)
        {
            if (client.Character.ChargeCurrent != client.Character.ChargeMax)
            {
                client.Character.ChargeCurrent = (long)Math.Max(0,
                    Math.Min(
                        client.Character.ChargeMax,
                        client.Character.ChargeCurrent + client.Character.ChargeRate * .1 * timeMultiplier)
                );
            }
        }
    }
}
