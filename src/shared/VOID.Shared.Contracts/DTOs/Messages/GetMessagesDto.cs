using VOID.Shared.Contracts.Enums.Chats;

namespace VOID.Shared.Contracts.DTOs.Messages;

public class GetMessagesDto
{
    public ChatType ParentType { get; set; }
}
