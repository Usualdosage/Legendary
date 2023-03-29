// <copyright file="Disarm.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models.Skills
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;

    /// <summary>
    /// Allows a player to disarm.
    /// </summary>
    public class Disarm : Skill
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Disarm"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public Disarm(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Disarm";
            this.ManaCost = 0;
            this.CanInvoke = true;
            this.IsAffect = false;
            this.AffectDuration = 0;
            this.DamageModifier = 0;
            this.HitDice = 0;
            this.DamageDice = 0;
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, Item? itemTarget, CancellationToken cancellationToken)
        {
            if (actor.Fighting.HasValue)
            {
                var player = this.Communicator.ResolveCharacter(actor.Fighting.Value);
                Character? character;

                if (player == null)
                {
                    character = this.Communicator.ResolveMobile(actor.Fighting.Value);
                }
                else
                {
                    character = player.Character;
                }

                if (character != null)
                {
                    var actorWeapon = actor.Equipment.FirstOrDefault(w => w.Key == Core.Types.WearLocation.Wielded);
                    var targetWeapon = character.Equipment.FirstOrDefault(w => w.Key == Core.Types.WearLocation.Wielded);

                    if (targetWeapon.Value == null)
                    {
                        await this.Communicator.SendToPlayer(actor, $"{character.FirstName} is not wielding a weapon.", cancellationToken);
                    }
                    else if (actorWeapon.Value == null)
                    {
                        await this.Communicator.SendToPlayer(actor, $"You are not wielding a weapon.", cancellationToken);
                    }
                    else
                    {
                        await this.Communicator.SendToPlayer(actor, $"You attempt to disarm {character.FirstName}.", cancellationToken);

                        // The chance to retain is the target's dex * 4.
                        var pctToRetain = character.Dex.Current * 4;

                        switch (actorWeapon.Value.WeaponType)
                        {
                            default:
                            case Core.Types.WeaponType.Exotic:
                                break;
                            case Core.Types.WeaponType.Polearm:
                            case Core.Types.WeaponType.Spear:
                                pctToRetain -= 20;
                                break;
                            case Core.Types.WeaponType.TwoHanded:
                            case Core.Types.WeaponType.Sword:
                                pctToRetain -= 15;
                                break;
                            case Core.Types.WeaponType.Mace:
                            case Core.Types.WeaponType.Axe:
                                pctToRetain -= 10;
                                break;
                            case Core.Types.WeaponType.Dagger:
                                pctToRetain -= 5;
                                break;
                            case Core.Types.WeaponType.Whip:
                            case Core.Types.WeaponType.Flail:
                                pctToRetain -= 25;
                                break;
                        }

                        if (this.Random.Next(0, 100) > pctToRetain)
                        {
                            if (!targetWeapon.Value.ItemFlags.Contains(ItemFlags.Cursed))
                            {
                                await this.Communicator.SendToPlayer(actor, $"You disarm {character.FirstName}!", cancellationToken);
                                await this.Communicator.SendToPlayer(character, $"{actor.FirstName} disarms you!", cancellationToken);
                                await this.Communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()} disarms {character.FirstName}!", cancellationToken);

                                character.Equipment.Remove(targetWeapon.Key);
                                var room = this.Communicator.ResolveRoom(actor.Location);

                                if (room != null)
                                {
                                    room.Items.Add(targetWeapon.Value.DeepCopy());
                                }
                            }
                            else
                            {
                                await this.Communicator.SendToPlayer(actor, $"You can't disarm {character.FirstName}!", cancellationToken);
                                await this.Communicator.SendToPlayer(character, $"{actor.FirstName} tries to disarm you, but fails.", cancellationToken);
                            }
                        }
                        else
                        {
                            await this.Communicator.SendToPlayer(actor, $"You fail to disarm {character.FirstName}.", cancellationToken);
                            await this.Communicator.SendToPlayer(character, $"{actor.FirstName} tries to disarm you, but fails.", cancellationToken);
                            await this.Communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()} tries to disarm {character.FirstName}, but fails.", cancellationToken);
                        }
                    }
                }
            }
            else
            {
                await this.Communicator.SendToPlayer(actor, $"You're not fighting anyone.", cancellationToken);
            }
        }
    }
}
