using Patrick.Models.Events;
using System;

namespace Patrick.Services.Implementation
{
    class EventPropagator : IEventPropagator
    {
        public event EventHandler<UserMentionEventArgs>? UserMentioned;

        public void ReportUserMentionEvent(UserMentionEventArgs eventArgs)
        {
            UserMentioned?.Invoke(this, eventArgs);
        }
    }
}
