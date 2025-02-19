using PdfSharp.Drawing;

namespace Benutzerverwaltungssoftware.Pdf;

internal static class Settings
{
    internal static class CPortrait
    {
        internal const int A4Height = 842;
        internal const int A4Width = 595;
    }
    internal static class CLandscape
    {
        internal const int A4Height = 595;
        internal const int A4Width = 842;
    }

    internal static class PageLayoutPortrait
    {
        internal static double VerticalStart { get; set; } = 60;
        internal static double VerticalEnd { get; set;} = 782;
        internal static double HorizontalStart { get; set;} = 60;
        internal static double HorizontalEnd { get; set;} = 535;
    }
    internal static class PageLayoutLandscape
    {
        internal static double VerticalStart { get; set; } = 60;
        internal static double VerticalEnd { get; set; } = 535;
        internal static double HorizontalStart { get; set; } = 60;
        internal static double HorizontalEnd { get; set; } = 782;
    }
}
internal class DocumentInformation
{
    internal required double LastVerticalPosition { get; set; }
    internal required double VerticalStart { get; set; }
    internal required double VerticalEnd { get; set; }
    internal required double HorizontalStart { get; set; }
    internal required double HorizontalEnd { get; set; }
    internal required List<XBrush> Brushes { get; set; }
    internal required List<XFont> Fonts { get; set; }
    internal required List<XPen> Pens { get; set; }
}
internal class CollectionInformation
{
    internal required double LastHorizontalPosition { get; set; }
    internal required double VerticalStart { get; set; }
    internal required double HorizontalStart { get; set; }
    internal required double HorizontalEnd { get; set; }
    internal required List<XBrush> Brushes { get; set; }
    internal required List<XFont> Fonts { get; set; }
    internal required List<XPen> Pens { get; set; }
    internal required double NeededHeight { get; set; }
}
internal class KeyValue
{
    internal required string Key { get; set; }
    internal required string Value { get; set; }
    internal required int IndexInFormatString { get; set; }
}
internal class Point
{
    internal required double VerticalPosition { get; set; }
    internal required double HorizontalPosition { get; set; }
}
internal class FormatString
{
    internal required string Format { get; set; }
    internal required List<KeyValue> KeyValues { get; set; }
}