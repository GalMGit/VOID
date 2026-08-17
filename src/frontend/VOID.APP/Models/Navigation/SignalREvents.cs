namespace VOID.APP.Models.Navigation;

public static class SignalREvents
{
    public const string SendTypingEvent = nameof(SendTypingEvent);
    public const string SendMessagesReadEvent = nameof(SendMessagesReadEvent);
    public const string JoinToGroupEvent = nameof(JoinToGroupEvent);
    public const string AddToGroupEvent = nameof(AddToGroupEvent);
    public const string RemoveFromGroupEvent = nameof(RemoveFromGroupEvent);
    public const string SendGroupMessagesReadEvent = nameof(SendGroupMessagesReadEvent);
    public const string LeaveFromGroupEvent = nameof(LeaveFromGroupEvent);
}