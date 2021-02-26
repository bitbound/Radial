using Radial.Models;
using Radial.Services.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Services
{
    public interface ICharacterEffectsService
    {
        void ApplyChargeRecovery(Location location, double timeMultiplier);
    }
    public class CharacterEffectsService : ICharacterEffectsService
    {
        public void ApplyChargeRecovery(Location location, double timeMultiplier)
        {
            foreach (var character in location.Characters)
            {
                if (character.ChargeCurrent != character.ChargeMax)
                {
                    character.ChargeCurrent = (long)Math.Max(0,
                        Math.Min(
                            character.ChargeMax,
                            character.ChargeCurrent + character.ChargeRate * .1 * timeMultiplier)
                    );
                }
            }

        }
    }
}
