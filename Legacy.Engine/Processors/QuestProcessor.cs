// <copyright file="QuestProcessor.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Processors
{
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;

    /// <summary>
    /// Handles processing of awards (quests) based on mobile output.
    /// </summary>
    public class QuestProcessor
    {
        private readonly ICommunicator communicator;
        private readonly ILogger logger;
        private readonly AwardProcessor awardProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestProcessor"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="communicator">The communicator.</param>
        /// <param name="awardProcessor">The award processor.</param>
        public QuestProcessor(ILogger logger, ICommunicator communicator, AwardProcessor awardProcessor)
        {
            this.logger = logger;
            this.communicator = communicator;
            this.awardProcessor = awardProcessor;
        }

        /// <summary>
        /// Checks for quest completion based on mobile speech output.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="actor">The player.</param>
        /// <param name="mobile">The mobile.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task CheckQuest(string input, Character actor, Mobile mobile, CancellationToken cancellationToken)
        {
            await this.CheckCassanovaQuest(input, actor, mobile, cancellationToken);
        }

        /// <summary>
        /// Converts this mobile to "adult" mode.
        /// </summary>
        /// <param name="message">The message to check for keywords.</param>
        /// <param name="actor">The actor who is the source.</param>
        /// <param name="mobile">The mobile.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task CheckCassanovaQuest(string message, Character actor, Mobile mobile, CancellationToken cancellationToken)
        {
            if (mobile.UseAI && mobile.XImages != null && mobile.XImages.Count > 0)
            {
                message = message.ToLower();

                if (message.Contains($"disrobes") || message.Contains($"naked body") || message.Contains($"removes {mobile.Pronoun} clothes") || message.Contains($"removes {mobile.Pronoun} clothing") || message.Contains($"takes off {mobile.Pronoun}") || message.Contains($"removes {mobile.Pronoun} shirt") || message.Contains($"removes {mobile.Pronoun} top") || message.Contains($"removes {mobile.Pronoun} pants") || message.Contains($"removes {mobile.Pronoun} panties"))
                {
                    if (mobile.XActive.HasValue && mobile.XActive.Value)
                    {
                        this.logger.Info($"{actor.FirstName} has activated {mobile.FirstName}.", this.communicator);
                        mobile.XActive = true;
                        await this.awardProcessor.GrantAward((int)Legendary.Core.Types.AwardType.Cassanova, actor, $"managed to see {mobile.FirstName} nude", cancellationToken);
                    }
                    else
                    {
                        mobile.XActive = false;
                    }
                }
                else if (message.Contains($"gets dressed") || message.Contains($"puts {mobile.Pronoun} clothes on") || message.Contains($"puts {mobile.Pronoun} clothes back on"))
                {
                    if (mobile.XActive.HasValue && mobile.XActive.Value)
                    {
                        this.logger.Info($"{actor.FirstName} has deactivated {mobile.FirstName}.", this.communicator);
                        await this.communicator.SendToPlayer(actor, $"{mobile.FirstName} gets dressed.", cancellationToken);
                        mobile.XActive = false;
                    }
                    else
                    {
                        mobile.XActive = false;
                    }
                }
                else if (message.Contains("cassanova-deactivate"))
                {
                    if (mobile.XActive.HasValue && mobile.XActive.Value)
                    {
                        this.logger.Info($"{actor.FirstName} has deactivated {mobile.FirstName}.", this.communicator);
                        await this.communicator.SendToPlayer(actor, $"{mobile.FirstName} seems turned off by your request and gets dressed.", cancellationToken);
                        mobile.XActive = false;
                    }
                    else
                    {
                        mobile.XActive = false;
                    }
                }
                else if (message.Contains("cassanova-activate"))
                {
                    this.logger.Info($"{actor.FirstName} has activated {mobile.FirstName}.", this.communicator);
                    mobile.XActive = true;
                    await this.awardProcessor.GrantAward((int)Legendary.Core.Types.AwardType.Cassanova, actor, $"managed to see {mobile.FirstName} nude", cancellationToken);
                    await this.communicator.SendToPlayer(actor, $"{mobile.FirstName} has removed {mobile.Pronoun} clothing.", cancellationToken);
                }
                else
                {
                    mobile.XActive = false;
                }
            }
        }
    }
}
