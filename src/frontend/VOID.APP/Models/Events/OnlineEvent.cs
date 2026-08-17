using System;

namespace VOID.APP.Models.Events;

public class OnlineEvent
{
    public Guid UserId { get; set; }
    public bool IsOnline { get; set; }
}