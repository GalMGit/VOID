using VOID.Shared.Contracts.Enums.Messages;

namespace VOID.Shared.Contracts.DTOs.Messages;

public class DeleteMessagesDto
{
    public List<Guid> MessageIds { get; set; } = [];
}