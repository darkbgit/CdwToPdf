using System.ComponentModel;

namespace CdwHelper.WPF;

public class OpenDoWorkEventsArgs : DoWorkEventArgs
{
    public OpenDoWorkEventsArgs(object? argument) : base(argument)
    {
    }

    public IEnumerable<string> FilesPaths { get; set; }
}