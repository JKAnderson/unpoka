namespace Unpoka;

internal class FriendlyException : Exception
{
    public FriendlyException(string message) : base(message) { }
}
