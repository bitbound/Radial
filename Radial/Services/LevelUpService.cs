using Radial.Models;
using Radial.Models.Messaging;
using System;

namespace Radial.Services
{
    public interface ILevelUpService
    {
        void AddRewardsFromWin(CharacterBase defeatedCharacter, Location location);
    }

    public class LevelUpService : ILevelUpService
    {
        private readonly IClientManager _clientManager;

        public LevelUpService(IClientManager clientManager)
        {
            _clientManager = clientManager;
        }

        public void AddRewardsFromWin(CharacterBase defeatedCharacter, Location location)
        {
            var glintToAdd = defeatedCharacter.Glint;
            var totalExp = defeatedCharacter.CorePower;

            _clientManager.SendToAllAtLocation(location, new LocalEventMessage($"You've gained {totalExp} experience and {glintToAdd} glint!", "text-success"));

            foreach (var player in location.PlayersAlive)
            {
                player.Glint += glintToAdd;
                var expGain = totalExp;
                while (expGain > 0)
                {
                    var expToAdd = Math.Min(expGain, player.CorePower - player.Experience);
                    player.Experience += expToAdd;

                    if (player.Experience == player.CorePower)
                    {
                        player.CorePower++;
                        player.EnergyCurrent = player.EnergyMax;
                        player.Experience = 0;
                        _clientManager.SendToPlayer(player, new LocalEventMessage($"Level up! Core energy increased to {player.CorePower}!", "text-success"));
                    }

                    expGain -= expToAdd;
                }
            }
        }
    }
}
