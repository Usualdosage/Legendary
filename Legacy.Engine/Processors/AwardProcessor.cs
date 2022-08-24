// <copyright file="AwardProcessor.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Processors
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Engine.Contracts;

    /// <summary>
    /// Handles processing of player awards (achievements).
    /// </summary>
    public class AwardProcessor
    {
        private readonly ICommunicator communicator;
        private readonly IWorld world;
        private readonly ILogger logger;
        private readonly IRandom random;
        private readonly Combat combat;

        /// <summary>
        /// Initializes a new instance of the <see cref="AwardProcessor"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="random">The random generator.</param>
        /// <param name="combat">The combat engine.</param>
        public AwardProcessor(ICommunicator communicator, IWorld world, ILogger logger, IRandom random, Combat combat)
        {
            this.communicator = communicator;
            this.world = world;
            this.logger = logger;
            this.random = random;
            this.combat = combat;
        }

        /// <summary>
        /// Grants a player the specified award.
        /// </summary>
        /// <param name="awardId">The award id.</param>
        /// <param name="actor">The player.</param>
        /// <param name="metaData">The metadata.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task GrantAward(int awardId, Character actor, string metaData, CancellationToken cancellationToken)
        {
            var playerAward = actor.Awards.Where(a => a.AwardId == awardId).FirstOrDefault();

            // Does player already have the award?
            if (playerAward != null)
            {
                if (playerAward.Metadata != null && !playerAward.Metadata.Contains(metaData))
                {
                    // See if we are leveling up this award. If we don't have a metadata entry that matches, this is a level up.
                    playerAward.Metadata?.Add(metaData);
                    playerAward.AwardLevel += 1;
                    await this.communicator.SendToPlayer(actor, $"<span class='award-message'>Congratulations, you have {metaData} and earned an award upgrade! Use \"awards\" to see your collection.</span>", cancellationToken);
                    await this.communicator.SendToPlayer(actor, $"You gain {playerAward.ExperiencePerLevel * (playerAward.AwardLevel + 1)} experience!", cancellationToken);
                    actor.Experience += playerAward.ExperiencePerLevel * playerAward.AwardLevel;
                }
                else
                {
                    // Something got messed up here.
                    return;
                }
            }
            else
            {
                var award = this.world.Awards.FirstOrDefault(a => a.AwardId == awardId);

                if (award != null)
                {
                    // New award, so grant at level 0 and apply the metadata, which is used to track progress.
                    award.AwardLevel = 0;
                    award.Metadata = new List<string>() { metaData };
                    actor.Awards.Add(award);
                    await this.communicator.SendToPlayer(actor, $"<span class='award-message'>Congratulations, you have {metaData} and earned a new bronze award! Use \"awards\" to see your collection.</span>", cancellationToken);
                    await this.communicator.SendToPlayer(actor, $"You gain {award.ExperiencePerLevel} experience!", cancellationToken);
                    actor.Experience += award.ExperiencePerLevel;
                }
            }

            await this.communicator.SaveCharacter(actor);
        }

        /// <summary>
        /// Check the voyager award.
        /// </summary>
        /// <param name="areaId">The area Id.</param>
        /// <param name="actor">The actor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task CheckVoyagerAward(long areaId, Character actor, CancellationToken cancellationToken)
        {
            var area = this.communicator.ResolveArea(areaId);

            if (area != null)
            {
                var roomsInArea = area.Rooms.Count();

                var areaExplored = actor.Metrics.RoomsExplored.Where(a => a.Key == area.AreaId);

                if (areaExplored != null)
                {
                    if (areaExplored.Count() == roomsInArea)
                    {
                        await this.GrantAward(7, actor, $"explored all rooms in {area.Name}", cancellationToken);
                    }
                }
            }
        }
    }
}