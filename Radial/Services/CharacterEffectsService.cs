using Radial.Models;
using System;

namespace Radial.Services
{
    public interface ICharacterEffectsService
    {
        void ApplyChargeRecovery(Location location, TimeSpan elapsed);
    }
    public class CharacterEffectsService : ICharacterEffectsService
    {
        public void ApplyChargeRecovery(Location location, TimeSpan elapsed)
        {
            foreach (var character in location.Characters)
            {
                if (character.ChargeCurrent != character.ChargeMax)
                {
                    character.ChargeCurrent = (long)Math.Max(0,
                        Math.Min(
                            character.ChargeMax,
                            Math.Ceiling(character.ChargeCurrent + character.ChargeRate * elapsed.TotalSeconds * .1))
                    );
                }
            }

        }
    }
}
