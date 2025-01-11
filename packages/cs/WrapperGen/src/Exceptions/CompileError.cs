using WrapperGen.Outputs;

namespace WrapperGen.Exceptions;

public class CompileError(
    string message,
    Diagnostics? diagnostics = null,
    Exception? innerException = null)
    : Exception(message, innerException)
{
    public Diagnostics Diagnostics { get; set; } = diagnostics ?? new();
}