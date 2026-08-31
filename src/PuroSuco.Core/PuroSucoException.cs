namespace PuroSuco.Core;

public sealed class PuroSucoException : Exception
{
    public string Code { get; }
    public string MemeTitle { get; }
    public int Position { get; }

    public PuroSucoException(string code, string memeTitle, string message, int position)
        : base(message)
    {
        Code = code;
        MemeTitle = memeTitle;
        Position = position;
    }

    public override string ToString() =>
        $"{Code} — {MemeTitle}\n{Message}\nPosição: {Position}";
}
