using PdfSharp.Drawing;

namespace Benutzerverwaltungssoftware.Pdf;

internal static class PdfSettings
{
    internal const int A4Height = 842;
    internal const int A4Width = 595;

    internal static class PageLayout
    {
        internal static double VerticalStart { get; set; } = 60;
        internal static double VerticalEnd { get; set;} = 782;
        internal static double HorizontalStart { get; set;} = 60;
        internal static double HorizontalEnd { get; set;} = 535;
    }
}
internal class PdfDrawingInformation
{
    internal required bool PreviousSuccess { get; set; }
    internal required double LastVerticalPosition { get; set; }
    internal required double VerticalStart { get; set; }
    internal required double VerticalEnd { get; set; }
    internal required double HorizontalStart { get; set; }
    internal required double HorizontalEnd { get; set; }
    internal required XBrush Brush { get; set; }
    internal required List<XFont> Fonts { get; set; }
    internal required XPen Pen { get; set; }
}
internal class PdfKeyValue
{
    internal required string Key { get; set; }
    internal required string Value { get; set; }
    internal required int IndexInFormatString { get; set; }
}
internal class PdfPoint
{
    internal required double VerticalPosition { get; set; }
    internal required double HorizontalPosition { get; set; }
}
internal class FormatString
{
    internal required string Format { get; set; }
    internal required List<PdfKeyValue> KeyValues { get; set; }
}