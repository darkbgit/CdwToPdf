namespace CdwHelper.Core.Exceptions;

public class AnalyzeException : Exception
{
    public AnalyzeException()
    {
    }

    public AnalyzeException(string massage)
        : base(massage)
    {
    }

    public AnalyzeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}