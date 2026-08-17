using System;
using System.Collections.Generic;

namespace VOID.APP.Models.Errors;

public class ErrorResponse
{
    public string Title { get; set; }
    public int StatusCode { get; set; }
    public string Instance { get; set; }
    public Dictionary<string, List<string>> Errors { get; set; }
    public string TraceId { get; set; }
}
