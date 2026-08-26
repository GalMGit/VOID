using System;

namespace VOID.Domain.Models.Users;

public class TempCode
{
    public string ConfirmationCode { get; set; }
    public DateTime CodeExpiresAt { get; set; }
}