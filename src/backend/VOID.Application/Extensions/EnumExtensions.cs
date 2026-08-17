using DomainChatType = VOID.Domain.Enums.Types.Chat.ChatType;
using SharedChatType = VOID.Shared.Contracts.Enums.Chats.ChatType;

using DomainMessageType = VOID.Domain.Enums.Types.Message.MessageType;
using SharedMessageType = VOID.Shared.Contracts.Enums.Messages.MessageType;
namespace VOID.Application.Extensions;

public static class EnumExtensions
{
    public static DomainChatType ToDomain(this SharedChatType value)
        => (DomainChatType)(int)value;

    public static SharedChatType ToShared(this DomainChatType value)
        => (SharedChatType)(int)value;

    public static DomainMessageType ToDomain(this SharedMessageType value)
        => (DomainMessageType)(int)value;

    public static SharedMessageType ToShared(this DomainMessageType value)
        => (SharedMessageType)(int)value;
}