// <copyright file="SpellProcessor.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Processors
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Helpers;
    using Legendary.Engine.Models;

    /// <summary>
    /// Used to perform quick lookups of spells.
    /// </summary>
    public class SpellProcessor
    {
        private readonly ICommunicator communicator;
        private readonly ILogger logger;
        private readonly IWorld world;
        private readonly ActionHelper actionHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpellProcessor"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="combat">The combat generator.</param>
        /// <param name="logger">The logger.</param>
        public SpellProcessor(ICommunicator communicator, IRandom random, IWorld world, Combat combat, ILogger logger)
        {
            this.communicator = communicator;
            this.actionHelper = new ActionHelper(communicator, random, world, logger, combat);
            this.logger = logger;
            this.world = world;
        }

        /// <summary>
        /// Executes the spell provided by the command.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="args">The input args.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DoSpell(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var proficiency = actor.Character.GetSpellProficiency(args.Method);

            if (proficiency != null && proficiency.Proficiency > 1)
            {
                var spell = this.actionHelper.CreateActionInstance<Spell>("Legendary.Engine.Models.Spells", proficiency.SpellName.Replace(" ", string.Empty));

                if (spell != null)
                {
                    // See if the player has enough mana to use this skill.
                    if (spell.ManaCost > actor.Character.Mana.Current)
                    {
                        await this.communicator.SendToPlayer(actor.Connection, "You don't have enough mana.", cancellationToken);
                        return;
                    }
                    else
                    {
                        // Had enough mana, so deduct from the current
                        actor.Character.Mana.Current -= spell.ManaCost;
                    }

                    Character? character = null;
                    Item? item = null;

                    if (!string.IsNullOrWhiteSpace(args.Target))
                    {
                        // We may or may not have a target. The skill will figure that bit out.
                        var target = this.communicator.ResolveCharacter(args.Target);

                        // It's not a character, so maybe a mob.
                        if (target == null)
                        {
                            var mob = this.communicator.ResolveMobile(args.Target);

                            if (mob != null)
                            {
                                character = (Character)mob;
                            }
                        }
                        else
                        {
                            character = target?.Character;
                        }

                        // Target could be an item.
                        item = this.communicator.ResolveItem(actor, args.Target);
                    }

                    if (await spell.IsSuccess(proficiency.Proficiency, cancellationToken))
                    {
                        try
                        {
                            await spell.PreAction(actor.Character, character, item, cancellationToken);
                            await spell.Act(actor.Character, character, item, cancellationToken);
                            await spell.PostAction(actor.Character, character, item, cancellationToken);
                        }
                        catch
                        {
                            throw;
                        }
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, "You lost your concentration.", cancellationToken);
                        await spell.CheckImprove(actor.Character, cancellationToken);
                        return;
                    }
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, "You don't know how to cast that spell.", cancellationToken);
                    return;
                }
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, "You don't know how to cast that spell.", cancellationToken);
                return;
            }
        }
    }
}