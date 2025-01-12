using Mediator;

namespace RatDefender.Application.Commands;

public record DeleteAllDetectionsCommand() : ICommand
{
    public static DeleteAllDetectionsCommand Instance { get; } = new();
}