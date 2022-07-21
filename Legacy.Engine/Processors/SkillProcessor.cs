// <copyright file="SkillProcessor.cs" company="Legendary™">
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
    using Legendary.Engine.Extensions;
    using Legendary.Engine.Helpers;
    using Legendary.Engine.Models;

    /// <summary>
    /// Used to perform quick lookups of skills.
    /// </summary>
    public class SkillProcessor
    {
        private readonly ICommunicator communicator;
        private readonly ILogger logger;
        private readonly ActionHelper actionHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillProcessor"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="combat">The combat generator.</param>
        /// <param name="logger">The logger.</param>
        public SkillProcessor(ICommunicator communicator, IRandom random, Combat combat, ILogger logger)
        {
            this.logger = logger;
            this.communicator = communicator;
            this.actionHelper = new ActionHelper(communicator, random, combat);
        }

        /// <summary>
        /// Executes the skill provided by the command.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="args">The input args.</param>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DoSkill(UserData actor, string[] args, string command, CancellationToken cancellationToken)
        {
            var proficiency = actor.Character.GetSkillProficiency(command);

            if (proficiency != null && proficiency.Proficiency > 0)
            {
                var skill = this.actionHelper.CreateActionInstance<Skill>("Legendary.Engine.Models.Skills", command.FirstCharToUpper());

                if (skill != null)
                {
                    // See if the player has enough mana to use this skill.
                    if (skill.ManaCost > actor.Character.Mana.Current)
                    {
                        await this.communicator.SendToPlayer(actor.Connection, "You don't have enough mana.", cancellationToken);
                        return;
                    }
                    else
                    {
                        // Had enough mana, so deduct from the current
                        actor.Character.Mana.Current -= skill.ManaCost;
                    }

                    var targetName = args.Length > 1 ? args[1] : string.Empty;

                    // We may or may not have a target. The skill will figure that bit out.
                    var target = Communicator.Users?.FirstOrDefault(u => u.Value.Username == targetName);

                    if (await skill.IsSuccess(proficiency.Proficiency, cancellationToken))
                    {
                        await skill.PreAction(actor.Character, target?.Value.Character, cancellationToken);
                        await skill.Act(actor.Character, target?.Value.Character, cancellationToken);
                        await skill.PostAction(actor.Character, target?.Value.Character, cancellationToken);
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, "You lost your concentration.", cancellationToken);
                        return;
                    }
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, "You don't know how to do that.", cancellationToken);
                    return;
                }
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, "You don't know how to do that.", cancellationToken);
                return;
            }
        }
    }
}