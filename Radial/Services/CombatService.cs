﻿using Radial.Enums;
using Radial.Models;
using Radial.Models.Messaging;
using Radial.Services.Client;
using Radial.Utilities;
using System;
using System.Linq;

namespace Radial.Services
{
    public interface ICombatService
    {
        void AttackTarget(CharacterBase npc, Location location);
        void Blast(CharacterBase attacker, Location location);

        void EvaluateCombatStates(Location location);

        void ExecuteNpcActions(Location location);
        void HealCharacter(CharacterBase healer,
            CharacterBase primaryRecipient,
            Location location);

        void InitiateNpcAttackOnSight(IClientConnection clientConnection);
        void InitiateNpcAttackOnSight(Location location);
        void ToggleGuard(CharacterBase character, Location location);
    }

    public class CombatService : ICombatService
    {
        private readonly IClientManager _clientManager;
        private readonly ILevelUpService _levelUpService;

        public CombatService(IClientManager clientManager, ILevelUpService levelUpService)
        {
            _clientManager = clientManager;
            _levelUpService = levelUpService;
        }

        public void AttackTarget(CharacterBase attacker, Location location)
        {
            if (!location.Characters.Contains(attacker.Target))
            {
                attacker.Target = null;
            }

            if (attacker.Target is null || attacker.Target?.Type == attacker.Type)
            {
                var enemies = location.CharactersAlive.Where(x => x.Type != attacker.Type);
                if (attacker is PlayerCharacter)
                {
                    enemies = enemies.Cast<Npc>().Where(x => !x.IsFriendly);
                }
                attacker.Target = Calculator.GetRandom(enemies);
            }

            if (attacker.Target is null)
            {
                return;
            }

            var attackPower = RollForAction(attacker, attacker.AttributeAttack);
            attacker.ChargeCurrent = 0;

            _clientManager.SendToAllAtLocation(location, new LocalEventMessage($"{attacker.Name} attacks {attacker.Target.Name}!", "text-danger"));

            attacker.State = CharacterState.InCombat;
            attacker.Target.State = CharacterState.InCombat;

            FindBlockersAndReceiveAttack(attacker, attacker.Target, location, attackPower);

        }

        public void Blast(CharacterBase attacker, Location location)
        {
            var targets = location.CharactersAlive.Where(x =>
                x.Type != attacker.Type);

            if (attacker is PlayerCharacter)
            {
                targets = targets.Cast<Npc>().Where(x => !x.IsFriendly);
            }

            if (!targets.Any())
            {
                return;
            }

            var attackPower = RollForAction(attacker, attacker.AttributeBlast);
            attacker.ChargeCurrent = 0;

            for (var i = 0; i < targets.Count(); i++)
            {
                attackPower *= .8;
            }

            attackPower = Math.Round(attackPower);

            foreach (var target in targets)
            {
                if (attacker is PlayerCharacter pc1)
                {
                    pc1.Stats.DamageDealt += (long)attackPower;
                }

                var overkill = attackPower - target.EnergyCurrent;
                target.EnergyCurrent = Math.Max(0, (long)(target.EnergyCurrent - attackPower));
                target.State = CharacterState.InCombat;

                _clientManager.SendToAllAtLocation(location,
                    new LocalEventMessage($"{target.Name} is blasted for {attackPower} damage! {target.EnergyPercentFormatted} remaining.", "text-danger"));

                if (target.EnergyCurrent < 1)
                {
                    if (attacker is PlayerCharacter pc2)
                    {
                        pc2.Stats.Kills++;
                    }
                    attacker.Target = null;
                    KillCharacter(target, overkill, location);
                }
            }
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
            if (!location.NpcsAlive.Any(x => x.State == CharacterState.InCombat))
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
                        var aggressiveNpcs = location.NpcsAlive.Where(x => 
                            x.State == CharacterState.InCombat ||
                            x.AggressionModel == AggressionModel.PlayerOnSight);
                            
                        if (aggressiveNpcs.Count() > 1 &&
                            !aggressiveNpcs.Any(x => x.IsGuarding))
                        {
                            ToggleGuard(npc, location);
                        }
                        else if (aggressiveNpcs.Count() == 1 &&
                            npc.IsGuarding)
                        {
                            ToggleGuard(npc, location);
                        }


                        if (npc.IsGuarding && npc.ChargeCurrent < 1)
                        {
                            continue;
                        }


                        var npcLowOnHealth = Calculator.GetRandom(location.Npcs.Where(x => x.EnergyPercent < .5));
                        // TODO: Decision tree based on attributes, other characters at the location, etc.
                        if (npcLowOnHealth is not null && Calculator.RollForBool(npc.ChargePercent))
                        {
                            HealCharacter(npc, npc, location);
                        }
                        else if (Calculator.RollForBool(npc.ChargePercent))
                        {
                            AttackTarget(npc, location);
                        }
                    }
                }
            }
        }

        public void HealCharacter(CharacterBase healer,
            CharacterBase primaryRecipient,
            Location location)
        {

            if (!location.Characters.Contains(primaryRecipient))
            {
                primaryRecipient = Calculator.GetRandom(location.CharactersAlive.Where(x => x.Type == healer.Type && x != healer));
            }

            if (primaryRecipient is null)
            {
                primaryRecipient = healer;
            }

            if (primaryRecipient.State == CharacterState.InCombat)
            {
                healer.State = CharacterState.InCombat;
            }

            var healPower = RollForAction(healer, healer.AttributeHeal);
            healer.ChargeCurrent = 0;

            var startEnergy = primaryRecipient.EnergyCurrent;
            primaryRecipient.EnergyCurrent = (long)Math.Min(primaryRecipient.EnergyMax, primaryRecipient.EnergyCurrent + healPower);

            if (primaryRecipient is PlayerCharacter primaryRec)
            {
                primaryRec.Stats.HealingReceived += primaryRecipient.EnergyCurrent - startEnergy;
            }

            if (healer is PlayerCharacter healerPC)
            {
                healerPC.Stats.HealingDone += primaryRecipient.EnergyCurrent - startEnergy;
            }


            if (healer == primaryRecipient)
            {
                _clientManager.SendToAllAtLocation(location, new LocalEventMessage($"{healer.Name} self-heals for {healPower}.", "text-success"));
            }
            else
            {
                _clientManager.SendToAllAtLocation(location, new LocalEventMessage($"{healer.Name} heals {primaryRecipient.Name} for {healPower}.", "text-success"));
            }

            var otherRecipients = location.CharactersAlive
                .Where(x => x.Type == healer.Type)
                .Except(new[] { primaryRecipient });

            var secondaryHealPower = healPower * .5;

            foreach (var character in otherRecipients)
            {
                if (character.State == CharacterState.InCombat)
                {
                    healer.State = CharacterState.InCombat;
                }

                startEnergy = character.EnergyCurrent;
                character.EnergyCurrent = (long)Math.Min(character.EnergyMax, character.EnergyCurrent + secondaryHealPower);

                if (healer is PlayerCharacter healerPC2)
                {
                    healerPC2.Stats.HealingDone += character.EnergyCurrent - startEnergy;
                }

                if (character is PlayerCharacter targetPC)
                {
                    targetPC.Stats.HealingReceived += character.EnergyCurrent - startEnergy;
                }

                if (healer == character)
                {
                    _clientManager.SendToAllAtLocation(location, new LocalEventMessage($"{healer.Name} self-heals for {secondaryHealPower}.", "text-success"));
                }
                else
                {
                    _clientManager.SendToAllAtLocation(location, new LocalEventMessage($"{healer.Name} heals {character.Name} for {secondaryHealPower}.", "text-success"));
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

        public void ToggleGuard(CharacterBase character, Location location)
        {
            character.IsGuarding = !character.IsGuarding;

            var verb = character.IsGuarding ? "starts" : "stops";

            _clientManager.SendToAllAtLocation(location, new LocalEventMessage($"{character.Name} {verb} guarding.", "text-info"));
        }

        private void FindBlockersAndReceiveAttack(CharacterBase attacker, CharacterBase target, Location location, double attackPower)
        {
            var remainingAttack = attackPower;

            var guardingCharacters = location.CharactersAlive.Where(x =>
                x.Type != attacker.Type &&
                x != attacker.Target &&
                x.IsGuarding);

            if (guardingCharacters.Any())
            {
                target = Calculator.GetRandom(guardingCharacters);
                target.State = CharacterState.InCombat;
                _clientManager.SendToAllAtLocation(location, new LocalEventMessage($"{target.Name} guards {attacker.Target.Name} from the attack.", "text-info"));
            }

            if (target.IsGuarding && target.ChargeCurrent > 0)
            {
                var blockAmountMod = RollForAction(target, target.AttributeGuard);

                var unblockablePercent = Math.Min(.5, Math.Max(.1, attackPower / blockAmountMod * .3));
                var unblockableAmount = Math.Round(attackPower * unblockablePercent);
                var blockableAmount = attackPower - unblockableAmount;

                remainingAttack = Math.Max(0, blockableAmount - blockAmountMod) + unblockableAmount;

                target.ChargeCurrent = (long)Math.Max(0, target.ChargeCurrent - blockableAmount);

                var blockedAmount = attackPower - remainingAttack;

                if (target is PlayerCharacter pc)
                {
                    pc.Stats.DamageBlocked += (long)blockedAmount;
                }

                _clientManager.SendToAllAtLocation(location, new LocalEventMessage($"{target.Name} blocks {blockedAmount} damage!", "text-info"));

                if (target.ChargeCurrent < 1)
                {
                    _clientManager.SendToAllAtLocation(location, new LocalEventMessage($"{target.Name}'s guard is shattered!", "text-danger"));
                }
            }

            var overkill = remainingAttack - target.EnergyCurrent;
            target.EnergyCurrent = (long)Math.Max(0, target.EnergyCurrent - remainingAttack);
            target.State = CharacterState.InCombat;

            if (target is PlayerCharacter targetPC)
            {
                targetPC.Stats.DamageReceived += (long)remainingAttack;
            }

            if (attacker is PlayerCharacter attackerPC)
            {
                attackerPC.Stats.DamageDealt += (long)attackPower;
            }

            _clientManager.SendToAllAtLocation(location,
                    new LocalEventMessage($"{target.Name} takes {remainingAttack} damage! {target.EnergyPercentFormatted} remaining.", "text-danger"));

            if (target.EnergyCurrent < 1)
            {
                if (attacker is PlayerCharacter attackerPC2)
                {
                    attackerPC2.Stats.Kills++;
                }

                attacker.Target = null;
                KillCharacter(target, overkill, location);
            }
        }

        private void KillCharacter(CharacterBase target, double overkillDamage, Location location)
        {
            target.State = CharacterState.Dead;

            if (target is PlayerCharacter pc)
            {
                pc.Stats.Deaths++;
            }

            var overkillPercentage = overkillDamage / target.EnergyMax;

            if (overkillPercentage < .5)
            {
                _clientManager.SendToAllAtLocation(location, new LocalEventMessage($"{target.Name} has DIED!", "text-danger"));
            }
            else if (overkillPercentage < .7)
            {
                _clientManager.SendToAllAtLocation(location, new LocalEventMessage($"{target.Name} FORCEFULLY EXPLODES from the death blow!", "text-danger"));
            }
            else if (overkillPercentage < .8)
            {
                _clientManager.SendToAllAtLocation(location, new LocalEventMessage($"{target.Name} is COMPLETELY BLOWN ASUNDER by the force of the attack!", "text-danger"));
            }
            else if (overkillPercentage < .9)
            {
                _clientManager.SendToAllAtLocation(location, new LocalEventMessage($"{target.Name} is UTTERLY DESTROYED by the powerful attack!", "text-danger"));
            }
            else
            {
                _clientManager.SendToAllAtLocation(location, new LocalEventMessage($"{target.Name} is IRREVOCABLY WIPED FROM EXISTENCE!", "text-danger"));
            }

            if (target is Npc npcTarget)
            {
                if (!npcTarget.IsRespawnable)
                {
                    location.RemoveCharacter(npcTarget);
                }

                _levelUpService.AddRewardsFromWin(target, location);
            }
        }

        private double RollForAction(CharacterBase attacker, long attributeBonus)
        {
            var totalCharge = attacker.ChargeCurrent + attributeBonus;

            if (attacker is PlayerCharacter)
            {
                return Math.Round(totalCharge * (double)Calculator.RandInstance.Next(75, 101) / 100);
            }
            else
            {
                return Math.Round(totalCharge * (double)Calculator.RandInstance.Next(50, 101) / 100);
            }
        }
    }
}
