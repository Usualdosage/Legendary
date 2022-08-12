// <copyright file="ActionProcessor2.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Processors
{
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Models;
    using Legendary.Engine.Models;

    /// <summary>
    /// Additional action processing commands.
    /// </summary>
    public partial class ActionProcessor
    {
        private async Task DoBuy(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"Not yet implemented.", cancellationToken);
        }

        private async Task DoList(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"Not yet implemented.", cancellationToken);
        }

        private async Task DoGain(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"Not yet implemented.", cancellationToken);
        }

        private async Task DoPractice(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"Not yet implemented.", cancellationToken);
        }

        private async Task DoPut(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"Not yet implemented.", cancellationToken);
        }

        private async Task DoSell(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"Not yet implemented.", cancellationToken);
        }

        private async Task DoTrain(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"Not yet implemented.", cancellationToken);
        }
    }
}