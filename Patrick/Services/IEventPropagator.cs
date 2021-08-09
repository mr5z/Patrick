using Patrick.Models.Events;
using System;

namespace Patrick.Services
{
    interface IEventPropagator
    {
        event EventHandler<UserMentionEventArgs>? UserMentioned;
        void ReportUserMentionEvent(UserMentionEventArgs eventArgs);
    }
}
