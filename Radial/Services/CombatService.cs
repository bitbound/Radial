using Radial.Enums;
using Radial.Models;
using Radial.Models.Messaging;
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
        void AttackTarget(CharacterBase npc, Location location, double actionBonus);
        void ExecuteNpcActions(Location location);
        void HealCharacter(CharacterBase healer,
            CharacterBase primaryRecipient,
            Location location,
            double actionBonus,
            IEnumerable<CharacterBase> otherRecipients);

        void InitiateNpcAttackOnSight(IClientConnection clientConnection);
        void InitiateNpcAttackOnSight(Location location);
        void EvaluateCombatStates(Location location);
    }

    public class CombatService : ICombatService
    {
        private readonly IClientManager _clientManager;

        public CombatService(IClientManager clientManager)
        {
            _clientManager = clientManager;
        }

        public void ApplyActionBonus(IClientConnection client, TimeSpan elapsed)
        {
            var actionBonusChange = elapsed.TotalSeconds;
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

        public void AttackTarget(CharacterBase attacker, Location location, double actionBonus)
        {
            if (attacker.Target is null || attacker.Target?.Type == attacker.Type)
            {
                attacker.Target = Calculator.GetRandom(location.CharactersAlive.Where(x => x.Type != attacker.Type));
            }

            if (attacker.Target is null)
            {
                return;
            }

            // TODO: Add attribute points when implemented.
            var attackPower = Math.Round(attacker.ChargeCurrent * (1 + actionBonus));
            attacker.ChargeCurrent = 0;

            _clientManager.SendToAllAtLocation(location, new LocalEventMessage($"{attacker.Name} attacks {attacker.Target.Name}!", "text-danger"));

            var target = attacker.Target;

            var guardingCharacters = location.CharactersAlive.Where(x =>
                x.Type == attacker.Type &&
                x != attacker.Target &&
                x.IsGuarding);

            if (guardingCharacters.Any())
            {
                target = Calculator.GetRandom(guardingCharacters);
                _clientManager.SendToAllAtLocation(location, new LocalEventMessage($"{target.Name} guards {attacker.Target.Name} from the attack.", "text-info"));
            }

            var wasGuarded = target.GuardAmount > 0;

            var remainingAttack = attackPower - target.GuardAmount;
            target.GuardAmount = (long)Math.Max(0, target.GuardAmount - attackPower);

            if (wasGuarded && target.GuardAmount < 1)
            {
                target.IsGuarding = false;
                _clientManager.SendToAllAtLocation(location, new LocalEventMessage($"{target.Name}'s guard is shattered!", "text-danger"));
            }

            target.EnergyCurrent = (long)Math.Max(0, target.EnergyCurrent - remainingAttack);

            _clientManager.SendToAllAtLocation(location,
                    new LocalEventMessage($"{target.Name} takes {remainingAttack} damage! {target.EnergyPercentFormatted} remaining.", "text-danger"));

            if (target.EnergyCurrent < 1)
            {
                target.State = CharacterState.Dead;
                _clientManager.SendToAllAtLocation(location, new LocalEventMessage($"{target.Name} has DIED!", "text-danger"));
                attacker.Target = null;
                if (target is Npc npcTarget && !npcTarget.IsRespawnable)
                {
                    location.Characters.Remove(npcTarget);
                }
            }
        }

        public void ExecuteNpcActions(Location location)
        {
            foreach (var npc in location.Npcs)
            {
                if (npc.State == CharacterState.InCombat)
                {
                    if (!location.PlayersAlive.Any())
                    {
                        npc.State = CharacterState.Normal;
                    }
                    else
                    {
                        var npcLowOnHealth = Calculator.GetRandom(location.Npcs.Where(x => x.EnergyPercent < .2));
                        // TODO: Decision tree based on attributes, other characters at the location, etc.
                        if (npcLowOnHealth is not null && Calculator.RollForBool(npc.ChargePercent))
                        {
                            HealCharacter(npc, npc, location, Calculator.RandInstance.NextDouble(), location.Npcs.Except(new[] { npc }));
                        }
                        else if (Calculator.RollForBool(npc.ChargePercent))
                        {
                            AttackTarget(npc, location, Calculator.RandInstance.NextDouble());
                        }
                    }
                }
            }
        }

        public void HealCharacter(CharacterBase healer,
            CharacterBase primaryRecipient,
            Location location,
            double actionBonus,
            IEnumerable<CharacterBase> otherRecipients)
        {
            if (healer.Target is null)
            {
                healer.Target = Calculator.GetRandom(location.CharactersAlive.Where(x => x.Type == healer.Type && x != healer));
            }

            if (healer.Target is null)
            {
                healer.Target = healer;
            }

            var healPower = Math.Round(healer.ChargeCurrent * (1 + actionBonus));
            healer.ChargeCurrent = 0;

            primaryRecipient.EnergyCurrent = (long)Math.Min(primaryRecipient.EnergyMax, primaryRecipient.EnergyMax + healPower);

            foreach (var character in otherRecipients.Except(new[] { healer.Target }))
            {
                character.EnergyCurrent = (long)Math.Min(primaryRecipient.EnergyMax, primaryRecipient.EnergyCurrent + healPower * .25);
            }

            if (healer == primaryRecipient)
            {
                _clientManager.SendToAllAtLocation(location, new LocalEventMessage($"{healer.Name} self-heals for {healPower}.", "text-success"));
            }
            else
            {
                _clientManager.SendToAllAtLocation(location, new LocalEventMessage($"{healer.Name} heals {primaryRecipient.Name} for {healPower}.", "text-success"));
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
                combatNpc.Target = client.Character;
            }
        }

        public void InitiateNpcAttackOnSight(Location location)
        {
            if (!location.PlayersAlive.Any())
            {
                return;
            }

            var combatNpc = location.Npcs.FirstOrDefault(x =>
               x.AggressionModel > AggressionModel.OnAttacked && x.State == CharacterState.Normal);

            if (combatNpc is null)
            {
                return;
            }

            var player = Calculator.GetRandom(location.PlayersAlive);
            player.State = CharacterState.InCombat;
            combatNpc.State = CharacterState.InCombat;
            combatNpc.Target = player;
        }

        public void EvaluateCombatStates(Location location)
        {
            if (!location.PlayersAlive.Any())
            {
                foreach (var npc in location.NpcsAlive)
                {
                    if (npc.State == CharacterState.InCombat)
                    {
                        npc.State = CharacterState.Normal;
                    }
                }
            }
            if (!location.NpcsAlive.Any(x=>x.State == CharacterState.InCombat))
            {
                foreach (var player in location.PlayersAlive)
                {
                    if (player.State == CharacterState.InCombat)
                    {
                        player.State = CharacterState.Normal;
                        player.LastCombatEncounter = DateTimeOffset.Now;
                    }
                }
            }
        }
    }
}
