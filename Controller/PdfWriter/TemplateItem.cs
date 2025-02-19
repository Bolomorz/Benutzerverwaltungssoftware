using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;

namespace Benutzerverwaltungssoftware.Pdf;

internal enum ItemMode { Static, StaticRepeat, Dynamic }
internal enum FileType { Document, Template }
internal enum GeometryType {Arc, Bezier, ClosedCurve, Curve, Ellipse, Line, Pie, Polygon, Rectangle, RoundedRectangle}

internal class Rect
{
    internal required double Top { get; set; }
    internal required double Left { get; set; }
    internal required double Bottom { get; set; }
    internal required double Right { get; set; }
}

internal interface ITemplateItem
{   
    void CalcParameters(DocumentItem doc, CollectionInformation drawinfo);
    void AdjustNewPage(double verticalstart);
    void Draw(DocumentItem doc, CollectionInformation drawinfo);
}
internal class TemplateItemCollection
{
    private ItemMode Mode;
    private List<ITemplateItem> Items;
    private double DistanceLeft, DistanceRight, DistanceTop;
    private CollectionInformation? DrawInfo;

    internal TemplateItemCollection(ItemMode mode, double distanceleft, double distanceright, double distancetop)
    {
        Mode = mode;
        DistanceLeft = distanceleft;
        DistanceRight = distanceright;
        DistanceTop = distancetop;
        Items = new();
    }

    internal bool DrawCollection(DocumentItem doc, DocumentInformation drawinfo)
    {
        DrawInfo = new()
        {
            VerticalStart = IsDynamic() ? drawinfo.LastVerticalPosition + DistanceTop : drawinfo.VerticalStart + DistanceTop,
            HorizontalStart = drawinfo.HorizontalStart + DistanceLeft,
            LastHorizontalPosition = drawinfo.HorizontalStart + DistanceLeft,
            HorizontalEnd = drawinfo.HorizontalEnd - DistanceRight,
            Brushes = drawinfo.Brushes,
            Pens = drawinfo.Pens,
            Fonts = drawinfo.Fonts,
            NeededHeight = 0
        };

        foreach(var item in Items) item.CalcParameters(doc, DrawInfo);
        var newpage = AdjustNewPage(doc, drawinfo);
        foreach(var item in Items) item.Draw(doc, DrawInfo);

        if(IsDynamic()) drawinfo.LastVerticalPosition += DistanceTop + DrawInfo.NeededHeight;

        return newpage;
    }

    internal void AddItem(ITemplateItem item) => Items.Add(item);
    internal bool IsStatic() => Mode is ItemMode.Static || Mode is ItemMode.StaticRepeat;
    internal bool IsStaticRepeat() => Mode is ItemMode.StaticRepeat;
    internal bool IsDynamic() => Mode is ItemMode.Dynamic;

    private bool AdjustNewPage(DocumentItem doc, DocumentInformation drawinfo)
    {
        if(IsStatic() || DrawInfo is null) return false;

        if(DrawInfo.VerticalStart + DrawInfo.NeededHeight > drawinfo.VerticalEnd)
        {
            doc.AddPage();
            foreach(var item in Items) item.AdjustNewPage(drawinfo.VerticalStart);
            drawinfo.LastVerticalPosition = drawinfo.VerticalStart;
            return true;
        }
        return false;
    }
}

internal class StringItem : ITemplateItem
{
    /// <summary>
    /// Static | StaticRepeat | Dynamic
    /// </summary>
    internal required ItemMode Mode { private get; set; }
    /// <summary>
    /// FormatString: 'pre{0}mid{1}next{0}post' for keyvalue from {0} to {n}<para/>
    /// KeyValue Pairs: Key[Value]: with index from {0} to {n} in FormatString
    /// </summary>
    internal required FormatString FormatString { private get; set; }
    /// <summary>
    /// distance from HorizontalStart: position = horizontalstart + distanceleft
    /// </summary>
    internal required double DistanceLeft { private get; set; } 
    /// <summary>
    /// distance from (Static: VerticalStart | Dynamic: LastVerticalPosition ): 
    /// position = (verticalstart||lastverticalposition) + distancetop
    /// </summary>
    internal required double DistanceTop { private get; set; }
    /// <summary>
    /// max width from startposition before linebreak occurs
    /// </summary>
    internal required double MaxWidth { private get; set; }
    /// <summary>
    /// index of used font in fontlist
    /// </summary>
    internal required int FontIndex { private get; set; } 
    /// <summary>
    /// index of used brush in brushlist
    /// </summary>
    internal required int BrushIndex { private get; set; } 
    /// <summary>
    /// index of used pen in penlist
    /// </summary>
    internal required int PenIndex { private get; set; }
    /// <summary>
    /// true: text will be underlined
    /// </summary>
    internal required bool Underline { private get; set; }

    private Information? Info;
    private class Information
    {
        internal required string Text { get; set; }
        internal required Rect Rect { get; set; }
        internal required XFont Font { get; set; }
    }

    internal StringItem(){}

    public void CalcParameters(DocumentItem doc, CollectionInformation drawinfo)
    {
        var full = doc.GetCurrentType() is FileType.Document ? GetFullString() : GetTemplateString();
        var verticalstart = drawinfo.VerticalStart + DistanceTop;
        var horizontalstart = Mode is ItemMode.Dynamic ? drawinfo.LastHorizontalPosition + DistanceLeft : drawinfo.HorizontalStart + DistanceLeft;
        if(full is null || full.Length == 0) full = " ";
        var font = drawinfo.Fonts[FontIndex];

        XRect rect = new(new XPoint(horizontalstart, 0), new XPoint(horizontalstart + MaxWidth, 800));
        TextMeasurements ptm = new(doc.GetCurrentXGraphics(), full, font, rect);
        var height = ptm.MeasureText();

        var width = doc.GetCurrentXGraphics().MeasureString(full, font).Width;

        drawinfo.NeededHeight = height > drawinfo.NeededHeight ? height : drawinfo.NeededHeight;

        Info = new(){ Text = full, Font = font, Rect = new(){Bottom = verticalstart + height, Top = verticalstart, Left = horizontalstart, Right = horizontalstart + width}};

        if(Mode == ItemMode.Dynamic) drawinfo.LastHorizontalPosition = Info.Rect.Right;
    }
    public void AdjustNewPage(double verticalstart)
    {
        if(Info is null) throw new Exception("calculate parameters before adjusting and drawing");
        
        double h = Info.Rect.Bottom - Info.Rect.Top;
        Info.Rect.Top = verticalstart;
        Info.Rect.Bottom = verticalstart + h;
    }
    public void Draw(DocumentItem doc, CollectionInformation drawinfo)
    {
        if(Info is null) throw new Exception("calculate parameters before adjusting and drawing");

        XRect rect = new XRect(new XPoint(Info.Rect.Left, Info.Rect.Top), new XPoint(Info.Rect.Right, Info.Rect.Bottom));
        XTextFormatter tf = new(doc.GetCurrentXGraphics());
        XStringFormat format = new(){LineAlignment = XLineAlignment.Near, Alignment = XStringAlignment.Near};
        tf.DrawString(Info.Text, Info.Font, drawinfo.Brushes[BrushIndex], rect, format);
        if(Underline)
        {
            XPoint p1 = new(Info.Rect.Left, Info.Rect.Bottom + 1);
            XPoint p2 = new(Info.Rect.Right, Info.Rect.Bottom + 1);
            doc.GetCurrentXGraphics().DrawLine(drawinfo.Pens[PenIndex], p1, p2);
        }
    }

    private string GetFullString()
    {
        var sort = FormatString.KeyValues.OrderBy(x => x.IndexInFormatString).ToList();
        var strings = new string[sort.Count];
        for(int i = 0; i < sort.Count; i++) strings[i] = sort[i].Value;
        return strings.Length > 0 ? string.Format(FormatString.Format, strings) : FormatString.Format;
    }
    private string GetTemplateString()
    {
        var sort = FormatString.KeyValues.OrderBy(x => x.IndexInFormatString).ToList();
        var strings = new string[sort.Count];
        for(int i = 0; i < sort.Count; i++) strings[i] = $"%[{sort[i].Key}]%";
        return strings.Length > 0 ? string.Format(FormatString.Format, strings) : FormatString.Format;
    }
}
internal class GeometryItem : ITemplateItem
{
    /// <summary>
    /// Static | StaticRepeat | Dynamic
    /// </summary>
    internal required ItemMode Mode { private get; set; }
    /// <summary>
    /// Arc, Bezier, ClosedCurve, Curve, Ellipse, Line, Pie, Polygon, Rectangle, RoundedRectangle
    /// </summary>
    internal required GeometryType Type { private get; set; }
    /// <summary>
    /// verticalposition: relative to startposition: startposition + verticalposition<para/>
    /// horizontalposition: relative to startposition: startposition + horizontalposition<para/>
    /// Bezier: 4 Points | ClosedCurve: 2 Points or more | Curve: 2 Points or more | Line: 2 Points | Polygon: 3 Points or more
    /// </summary>
    internal required List<Point> Points { private get; set; }
    /// <summary>
    /// ClosedCurve | Polygon
    /// </summary>
    internal required XFillMode FillMode { private get; set; }
    /// <summary>
    /// distance from HorizontalStart: startposition = horizontalstart + distanceleft<para/>
    /// Rectangle | RoundedRectangle | Pie | Ellipse | Arc
    /// </summary>
    internal required double DistanceLeft { private get; set; } 
    /// <summary>
    /// distance from (Static: VerticalStart | Dynamic: LastVerticalPosition ): 
    /// startposition = (verticalstart||lastverticalposition) + distancetop<para/>
    /// !!!needed for all geometries!!!
    /// </summary>
    internal required double DistanceTop { private get; set; } 
    /// <summary>
    /// height from startposition: startposition + height<para/>
    /// Rectangle | RoundedRectangle | Pie | Ellipse | Arc
    /// </summary>
    internal required double Height { private get; set; } 
    /// <summary>
    /// width from startposition: startposition + width<para/>
    /// Rectangle | RoundedRectangle | Pie | Ellipse | Arc
    /// </summary>
    internal required double Width { private get; set; }
    /// <summary>
    /// Arc | Pie
    /// </summary>
    internal required double StartAngle { private get; set; } 
    /// <summary>
    /// Arc | Pie
    /// </summary>
    internal required double SweepAngle { private get; set; }
    /// <summary>
    /// ClosedCurve | Curve
    /// </summary>
    internal required double Tension { private get; set; }
    /// <summary>
    /// RoundedRectangle
    /// </summary>
    internal required double EllipseWidth { private get; set; } 
    /// <summary>
    /// RoundedRectangle
    /// </summary>
    internal required double EllipseHeight { private get; set; }
    /// <summary>
    /// index of brush in brushlist
    /// </summary>
    internal required int BrushIndex { private get; set; } 
    /// <summary>
    /// index of pen in penlist
    /// </summary>
    internal required int PenIndex { private get; set; }

    private Information? Info;
    private class Information
    {
        internal required Rect Rect { get; set; }
    }

    internal GeometryItem(){}

    public void CalcParameters(DocumentItem doc, CollectionInformation drawinfo)
    {
        if(!TestPointCount()) throw new Exception("geometry doesnt have required amount of points");

        var verticalstart = drawinfo.VerticalStart + DistanceTop;
        var horizontalstart = Mode is ItemMode.Dynamic ? drawinfo.LastHorizontalPosition + DistanceLeft : drawinfo.HorizontalStart + DistanceLeft;

        var height = DistanceTop + CalculateHeight();
        var width = CalculateWidth();

        drawinfo.NeededHeight = height > drawinfo.NeededHeight ? height : drawinfo.NeededHeight;

        Info = new(){Rect = new(){Top = verticalstart, Bottom = verticalstart + height, Left = horizontalstart, Right = horizontalstart + width}};

        if(Mode == ItemMode.Dynamic) drawinfo.LastHorizontalPosition = Info.Rect.Right;
    }
    public void AdjustNewPage(double verticalstart)
    {
        if(Info is null) throw new Exception("calculate parameters before adjusting and drawing");

        double h = Info.Rect.Bottom - Info.Rect.Top;
        Info.Rect.Top = verticalstart;
        Info.Rect.Bottom = verticalstart + h;
    }
    public void Draw(DocumentItem doc, CollectionInformation drawinfo)
    {
        if(Info is null) throw new Exception("calculate parameters before adjusting and drawing");
        if(!TestPointCount()) throw new Exception("geometry doesnt have required amount of points");

        DrawGeometry(doc.GetCurrentXGraphics(), drawinfo.Pens[PenIndex], drawinfo.Brushes[BrushIndex]);
    }

    private void DrawGeometry(XGraphics gfx, XPen pen, XBrush brush)
    {
        if(Info is null) throw new Exception("calculate parameters before adjusting and drawing");
        XPoint[] ps;
        XRect rect;
        switch(Type)
        {
            case GeometryType.Arc:
            rect = new(new XPoint(Info.Rect.Left, Info.Rect.Top), new XPoint(Info.Rect.Right, Info.Rect.Bottom));
            gfx.DrawArc(pen, rect, StartAngle, SweepAngle);
            break;
            case GeometryType.Bezier:
            ps = new XPoint[4];
            for(int i = 0; i < 4; i++) ps[i] = new(Points[i].HorizontalPosition + Info.Rect.Left, Points[i].VerticalPosition + Info.Rect.Top);
            gfx.DrawBezier(pen, ps[0], ps[1], ps[2], ps[3]);
            break;
            case GeometryType.ClosedCurve:
            ps = new XPoint[Points.Count];
            for(int i = 0; i < Points.Count; i++) ps[i] = new(Points[i].HorizontalPosition + Info.Rect.Left, Points[i].VerticalPosition + Info.Rect.Top);
            gfx.DrawClosedCurve(pen, brush, ps, FillMode, Tension);
            break;
            case GeometryType.Curve:
            ps = new XPoint[Points.Count];
            for(int i = 0; i < Points.Count; i++) ps[i] = new(Points[i].HorizontalPosition + Info.Rect.Left, Points[i].VerticalPosition + Info.Rect.Top);
            gfx.DrawCurve(pen, ps, Tension);
            break;
            case GeometryType.Ellipse:
            rect = new(new XPoint(Info.Rect.Left, Info.Rect.Top), new XPoint(Info.Rect.Right, Info.Rect.Bottom));
            gfx.DrawEllipse(pen, rect);
            break;
            case GeometryType.Line:
            ps = new XPoint[2];
            for(int i = 0; i < 2; i++) ps[i] = new(Points[i].HorizontalPosition + Info.Rect.Left, Points[i].VerticalPosition + Info.Rect.Top);
            gfx.DrawLine(pen, ps[0], ps[1]);
            break;
            case GeometryType.Pie:
            rect = new(new XPoint(Info.Rect.Left, Info.Rect.Top), new XPoint(Info.Rect.Right, Info.Rect.Bottom));
            gfx.DrawPie(pen, rect, StartAngle, SweepAngle);
            break;
            case GeometryType.Polygon:
            ps = new XPoint[Points.Count];
            for(int i = 0; i < Points.Count; i++) ps[i] = new(Points[i].HorizontalPosition + Info.Rect.Left, Points[i].VerticalPosition + Info.Rect.Top);
            gfx.DrawPolygon(brush, ps, FillMode);
            break;
            case GeometryType.Rectangle:
            rect = new(new XPoint(Info.Rect.Left, Info.Rect.Top), new XPoint(Info.Rect.Right, Info.Rect.Bottom));
            gfx.DrawRectangle(pen, rect);
            break;
            case GeometryType.RoundedRectangle:
            rect = new(new XPoint(Info.Rect.Left, Info.Rect.Top), new XPoint(Info.Rect.Right, Info.Rect.Bottom));
            gfx.DrawRoundedRectangle(pen, rect, new XSize(EllipseWidth, EllipseHeight));
            break;
        }
    }
    private bool TestPointCount()
    {
        switch(Type)
        {
            case GeometryType.Bezier: return Points.Count == 4;
            case GeometryType.ClosedCurve: return Points.Count >= 2;
            case GeometryType.Curve: return Points.Count >= 2;
            case GeometryType.Line: return Points.Count == 2;
            case GeometryType.Polygon: return Points.Count >= 3;
            default: return true;
        }
    }
    private double CalculateHeight()
    {
        double max = 0;
        switch(Type)
        {
            case GeometryType.Arc: return Height;
            case GeometryType.Bezier: foreach(var p in Points) if(p.VerticalPosition > max) max = p.VerticalPosition; return max;
            case GeometryType.ClosedCurve: foreach(var p in Points) if(p.VerticalPosition > max) max = p.VerticalPosition; return max;
            case GeometryType.Curve: foreach(var p in Points) if(p.VerticalPosition > max) max = p.VerticalPosition; return max;
            case GeometryType.Ellipse: return Height;
            case GeometryType.Line: foreach(var p in Points) if(p.VerticalPosition > max) max = p.VerticalPosition; return max;
            case GeometryType.Pie: return Height;
            case GeometryType.Polygon: foreach(var p in Points) if(p.VerticalPosition > max) max = p.VerticalPosition; return max;
            case GeometryType.Rectangle: return Height;
            case GeometryType.RoundedRectangle: return Height;
            default: return 0;
        }
    }
    private double CalculateWidth()
    {
        double max = 0;
        switch(Type)
        {
            case GeometryType.Arc: return Width;
            case GeometryType.Bezier: foreach(var p in Points) if(p.HorizontalPosition > max) max = p.HorizontalPosition; return max;
            case GeometryType.ClosedCurve: foreach(var p in Points) if(p.HorizontalPosition > max) max = p.HorizontalPosition; return max;
            case GeometryType.Curve: foreach(var p in Points) if(p.HorizontalPosition > max) max = p.HorizontalPosition; return max;
            case GeometryType.Ellipse: return Width;
            case GeometryType.Line: foreach(var p in Points) if(p.HorizontalPosition > max) max = p.HorizontalPosition; return max;
            case GeometryType.Pie: return Width;
            case GeometryType.Polygon: foreach(var p in Points) if(p.HorizontalPosition > max) max = p.HorizontalPosition; return max;
            case GeometryType.Rectangle: return Width;
            case GeometryType.RoundedRectangle: return Width;
            default: return 0;
        }
    }
}
internal class ImageItem : ITemplateItem
{
    /// <summary>
    /// Static | StaticRepeat | Dynamic
    /// </summary>
    internal required ItemMode Mode { private get; set; }
    /// <summary>
    /// image data as byte[]
    /// </summary>
    internal required byte[] ImageBytes { private get; set; }
    /// <summary>
    /// distance from HorizontalStart: startposition = horizontalstart + distanceleft
    /// </summary>
    internal required double DistanceLeft { private get; set; }
    /// <summary>
    /// distance from (Static: VerticalStart | Dynamic: LastVerticalPosition ): 
    /// startposition = (verticalstart||lastverticalposition) + distancetop
    /// </summary>
    internal required double DistanceTop { private get; set; } 
    /// <summary>
    /// width from startposition: startposition + width
    /// </summary>
    internal required double Width { private get; set; } 
    /// <summary>
    /// height from startposition: startposition + height
    /// </summary>
    internal required double Height { private get; set; }

    private Information? Info;
    private class Information
    {
        internal required Rect Rect { get; set; }
    }

    internal ImageItem(){}

    public void CalcParameters(DocumentItem doc, CollectionInformation drawinfo)
    {
        var verticalstart = drawinfo.VerticalStart + DistanceTop;
        var horizontalstart = Mode is ItemMode.Dynamic ? drawinfo.LastHorizontalPosition + DistanceLeft : drawinfo.HorizontalStart + DistanceLeft;

        drawinfo.NeededHeight = Height > drawinfo.NeededHeight ? Height : drawinfo.NeededHeight;

        Info = new(){Rect = new(){Top = verticalstart, Bottom = verticalstart + Height, Left = horizontalstart, Right = horizontalstart + Width}};

        if(Mode == ItemMode.Dynamic) drawinfo.LastHorizontalPosition = Info.Rect.Right;
    }
    public void AdjustNewPage(double verticalstart)
    {
        if(Info is null) throw new Exception("calculate parameters before adjusting and drawing");

        double h = Info.Rect.Bottom - Info.Rect.Top;
        Info.Rect.Top = verticalstart;
        Info.Rect.Bottom = verticalstart + h;
    }
    public void Draw(DocumentItem doc, CollectionInformation drawinfo)
    {
        if(Info is null) throw new Exception("calculate parameters before adjusting and drawing");

        XRect rect = new(new XPoint(Info.Rect.Left, Info.Rect.Top), new XPoint(Info.Rect.Right, Info.Rect.Bottom));

        using(var stream = new MemoryStream(ImageBytes, 0, ImageBytes.Length, true, true))
        {
            XImage img = XImage.FromStream(stream);
            doc.GetCurrentXGraphics().DrawImage(img, rect);
        }
    }

    internal static byte[] ImageFileToBytes(string path) => File.ReadAllBytes(path);
}