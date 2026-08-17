using VOID.Shared.Contracts.Enums.Chats;
using VOID.Shared.Contracts.Enums.Messages;

namespace VOID.Shared.Contracts.DTOs.Messages;

public class CreateMessageDto
{
    public string? Text { get; set; }
    public Guid ParentId { get; set; }
    public MessageType MessageType { get; set; } = MessageType.Text;
    public ChatType ChatType { get; set; } = ChatType.Private;
}
    
