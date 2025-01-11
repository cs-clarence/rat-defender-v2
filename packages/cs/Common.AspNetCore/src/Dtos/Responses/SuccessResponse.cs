namespace Common.AspNetCore.Dtos.Responses;

public record SuccessResponse(string Message = "Success")
    : MessageResponse(Message);
