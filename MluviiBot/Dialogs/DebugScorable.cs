﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Scorables;
using Microsoft.Bot.Connector;
#pragma warning disable 1998

namespace MluviiBot.Dialogs
{
    public class DebugScorable : IScorable<IActivity, double>
    {
        private readonly IMluviiBotDialogFactory dialogFactory;
        private readonly IDialogTask task;

        public DebugScorable(IDialogTask task, IMluviiBotDialogFactory dialogFactory)
        {
            SetField.NotNull(out this.task, nameof(task), task);
            SetField.NotNull(out this.dialogFactory, nameof(dialogFactory), dialogFactory);
        }

        public async Task<object> PrepareAsync(IActivity item, CancellationToken token)
        {
            var message = item as IMessageActivity;

            if (!string.IsNullOrWhiteSpace(message?.Text))
                if (message.Text == "618" || message.Text == "hadooken")
                    return message.Text;

            return null;
        }

        public bool HasScore(IActivity item, object state)
        {
            return state != null;
        }

        public double GetScore(IActivity item, object state)
        {
            var matched = state != null;
            var score = matched ? 1.0 : double.NaN;
            return score;
        }

        public async Task PostAsync(IActivity item, object state, CancellationToken token)
        {
            var message = item as IMessageActivity;

            if (message != null)
            {
                var DebugDialog = dialogFactory.Create<DebugDialog>();

                // wrap it with an additional dialog that will restart the wait for
                // messages from the user once the child dialog has finished
                var interruption = DebugDialog.Void<object, IMessageActivity>();

                // put the interrupting dialog on the stack
                task.Call(interruption, null);

                // start running the interrupting dialog
                await task.PollAsync(token);
            }
        }

        public Task DoneAsync(IActivity item, object state, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}