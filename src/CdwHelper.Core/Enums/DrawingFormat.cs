namespace CdwHelper.Core.Enums;

[Flags]
public enum DrawingFormat
{
    Undefined = 0,
    A0 = 1 << 0,
    A1 = 1 << 1,
    A2 = 1 << 2,
    A3 = 1 << 3,
    A4 = 1 << 4,
    A5 = 1 << 5,
    All = A0 | A1 | A2 | A3 | A4 | A5,
}