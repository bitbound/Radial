using Radial.Models;
using Radial.Models.Messaging;
using Radial.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var totalExp = defeatedCharacter.CoreEnergy;

            _clientManager.SendToAllAtLocation(location, new LocalEventMessage($"You've gained {totalExp} experience and {glintToAdd} glint!", "text-success"));

            foreach (var player in location.PlayersAlive)
            {
                player.Glint += glintToAdd;
                var expGain = totalExp;
                while (expGain > 0)
                {
                    var expToAdd = Math.Min(expGain, player.CoreEnergy - player.Experience);
                    player.Experience += expToAdd;

                    if (player.Experience == player.CoreEnergy)
                    {
                        player.CoreEnergy++;
                        player.Experience = 0;
                        _clientManager.SendToPlayer(player, new LocalEventMessage($"Core energy increased to {player.CoreEnergy}!", "text-success"));
                    }

                    expGain -= expToAdd;
                }
            }
        }
    }
}
