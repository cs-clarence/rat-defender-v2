using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Common.AspNetCore.Dtos.Responses;

public class DomainProblemDetails : ProblemDetails
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("domain")]
    public required string Domain { get; set; }
}
