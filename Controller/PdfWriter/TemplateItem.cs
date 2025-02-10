using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;

namespace Benutzerverwaltungssoftware.Pdf;

internal enum ItemMode { Static, Dynamic }
internal enum FileType { Document, Template }
internal enum GeometryType {Arc, Bezier, ClosedCurve, Curve, Ellipse, Line, Pie, Polygon, Rectangle, RoundedRectangle}

internal class PdfRect
{
    internal required double Top { get; set; }
    internal required double Left { get; set; }
    internal required double Bottom { get; set; }
    internal required double Right { get; set; }
}
internal interface ITemplateItem
{   
    void CalcParameters(PdfDocumentItem doc, PdfDrawingInformation drawinfo);
    void AdjustNewPage(PdfDocumentItem doc, PdfDrawingInformation drawinfo);
    void Draw(PdfDocumentItem doc, PdfDrawingInformation drawinfo);
}

internal class PdfStringItem : ITemplateItem
{
    private ItemMode Mode;
    private FormatString FormatString;
    private double DistanceLeft, DistanceRight, DistanceTop;
    private int FontIndex;
    private bool Underline;

    private Information? Info;
    private class Information
    {
        internal required string Text;
        internal required PdfRect Rect;
        internal required XFont Font;
    }

    /// <summary>
    /// create StringItem: si <para/>!!!following sequence is important!!!:<para/> si.CalcParameters();<para/>si.AdjustNewPage();<para/>si.Draw();
    /// </summary>
    /// <param name="mode">Static | Dynamic</param>
    /// <param name="format">FormatString: 'pre{0}mid{1}next{0}post' for keyvalue from {0} to {n}<para/>KeyValue Pairs: Key[Value]: with index from {0} to {n} in FormatString</param>
    /// <param name="distanceleft">distance from HorizontalStart: position = horizontalstart + distanceleft</param>
    /// <param name="distanceright">distance from HorizontalEnd: position = horizontalend - distanceright</param>
    /// <param name="distancetop">distance from (Static: VerticalStart | Dynamic: LastVerticalPosition ): position = (verticalstart||lastverticalposition) + distancetop</param>
    /// <param name="fontindex">index of used font in fontlist</param>
    /// <param name="underline">true: text will be underlined</param>
    internal PdfStringItem(ItemMode mode, FormatString format, double distanceleft, double distanceright, double distancetop, int fontindex, bool underline)
    {
        Mode = mode;
        FormatString = format;
        DistanceLeft = distanceleft;
        DistanceRight = distanceright;
        DistanceTop = distancetop;
        Underline = underline;
        FontIndex = fontindex;
    }

    public void CalcParameters(PdfDocumentItem doc, PdfDrawingInformation drawinfo)
    {
        var full = doc.GetCurrentType() == FileType.Document ? GetFullString() : GetTemplateString();
        var verticalstart = Mode == ItemMode.Dynamic ? drawinfo.LastVerticalPosition + DistanceTop : drawinfo.VerticalStart + DistanceTop;
        if(full is null || full.Length == 0) full = " ";
        var font = drawinfo.Fonts[FontIndex];

        XRect rect = new(new XPoint(drawinfo.HorizontalStart + DistanceLeft, 0), new XPoint(drawinfo.HorizontalEnd - DistanceRight, 800));
        PdfTextMeasurements ptm = new(doc.GetCurrentXGraphics(), full, font, rect);
        int l; double h; ptm.MeasureText(out l, out h);

        Info = new(){ Text = full, Font = font, Rect = new(){Bottom = verticalstart + h, Top = verticalstart, Left = drawinfo.HorizontalStart + DistanceLeft, Right = drawinfo.HorizontalEnd - DistanceRight}};
    }
    public void AdjustNewPage(PdfDocumentItem doc, PdfDrawingInformation drawinfo)
    {
        if(Info is null) throw new Exception("calculate parameters before adjusting and drawing");
        if(Mode == ItemMode.Static) return;
        if(Info.Rect.Bottom > drawinfo.VerticalEnd)
        {
            doc.AddPage();
            double h = Info.Rect.Bottom - Info.Rect.Top;
            Info.Rect.Top = drawinfo.VerticalStart;
            Info.Rect.Bottom = drawinfo.VerticalStart + h;
            drawinfo.LastVerticalPosition = drawinfo.VerticalStart;
        }
    }
    public void Draw(PdfDocumentItem doc, PdfDrawingInformation drawinfo)
    {
        if(Info is null) throw new Exception("calculate parameters before adjusting and drawing");
        XRect rect = new XRect(new XPoint(Info.Rect.Left, Info.Rect.Top), new XPoint(Info.Rect.Right, Info.Rect.Bottom));
        XTextFormatter tf = new(doc.GetCurrentXGraphics());
        XStringFormat format = new(){LineAlignment = XLineAlignment.Near, Alignment = XStringAlignment.Near};
        tf.DrawString(Info.Text, Info.Font, drawinfo.Brush, rect, format);
        if(Underline)
        {
            XPoint p1 = new(Info.Rect.Left, Info.Rect.Bottom + 1);
            XPoint p2 = new(Info.Rect.Right, Info.Rect.Bottom + 1);
            doc.GetCurrentXGraphics().DrawLine(drawinfo.Pen, p1, p2);
        }
        if(Mode == ItemMode.Dynamic) drawinfo.LastVerticalPosition = Info.Rect.Bottom;
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

internal class PdfGeometryItem : ITemplateItem
{
    private ItemMode Mode;
    private GeometryType Type;
    private List<PdfPoint> Points;
    private XFillMode FillMode;
    private double DistanceLeft, DistanceRight, DistanceTop, Height;
    private double StartAngle, SweepAngle, Tension, EllipseWidth, EllipseHeight;

    private Information? Info;
    private class Information
    {
        internal required PdfRect Rect;
    }

    /// <summary>
    /// create GeometryItem: gi <para/>!!!following sequence is important!!!:<para/> gi.CalcParameters();<para/>gi.AdjustNewPage();<para/>gi.Draw();
    /// </summary>
    /// <param name="mode">Static | Dynamic</param>
    /// <param name="type">Arc, Bezier, ClosedCurve, Curve, Ellipse, Line, Pie, Polygon, Rectangle, RoundedRectangle</param>
    /// <param name="distanceleft">distance from HorizontalStart: startposition = horizontalstart + distanceleft<para/>
    /// Rectangle | RoundedRectangle | Pie | Ellipse | Arc</param>
    /// <param name="distanceright">distance from HorizontalEnd: endposition = horizontalend - distanceright<para/>
    /// Rectangle | RoundedRectangle | Pie | Ellipse | Arc</param>
    /// <param name="distancetop">distance from (Static: VerticalStart | Dynamic: LastVerticalPosition ): startposition = (verticalstart||lastverticalposition) + distancetop<para/>
    /// !!!needed for all geometries!!!</param>
    /// <param name="height">height from startposition: startposition + height<para/>
    /// Rectangle | RoundedRectangle | Pie | Ellipse | Arc</param>
    /// <param name="points">verticalposition: relative to startposition: startposition + verticalposition<para/>horizontalposition: relative to startposition: startposition + horizontalposition<para/>
    /// Bezier: 4 Points | ClosedCurve: 2 Points or more | Curve: 2 Points or more | Line: 2 Points | Polygon: 3 Points or more</param>
    /// <param name="fillmode">ClosedCurve | Polygon</param>
    /// <param name="startangle">Arc | Pie</param>
    /// <param name="sweepangle">Arc | Pie</param>
    /// <param name="tension">ClosedCurve | Curve</param>
    /// <param name="ellipsewidth">RoundedRectangle</param>
    /// <param name="ellipseheight">RoundedRectangle</param>
    internal PdfGeometryItem(ItemMode mode, GeometryType type, double distanceleft, double distanceright, double distancetop, double height, List<PdfPoint> points,
        XFillMode fillmode, double startangle, double sweepangle, double tension, double ellipsewidth, double ellipseheight)
    {
        Mode = mode;
        Type = type;
        DistanceLeft = distanceleft;
        DistanceRight = distanceright;
        DistanceTop = distancetop;
        Height = height;
        Points = points;
        FillMode = fillmode;
        StartAngle = startangle;
        SweepAngle = sweepangle;
        Tension = tension;
        EllipseWidth = ellipsewidth;
        EllipseHeight = ellipseheight;
    }

    public void CalcParameters(PdfDocumentItem doc, PdfDrawingInformation drawinfo)
    {
        var verticalstart = Mode == ItemMode.Dynamic ? drawinfo.LastVerticalPosition + DistanceTop : drawinfo.VerticalStart + DistanceTop;

        var height = CalculateHeight();

        Info = new(){Rect = new(){Top = verticalstart, Bottom = verticalstart + height, Left = drawinfo.HorizontalStart + DistanceLeft, Right = drawinfo.HorizontalEnd - DistanceRight}};
    }
    public void AdjustNewPage(PdfDocumentItem doc, PdfDrawingInformation drawinfo)
    {
        if(Info is null) throw new Exception("calculate parameters before adjusting and drawing");
        if(Mode == ItemMode.Static) return;
        if(Info.Rect.Bottom > drawinfo.VerticalEnd)
        {
            doc.AddPage();
            double h = Info.Rect.Bottom - Info.Rect.Top;
            Info.Rect.Top = drawinfo.VerticalStart;
            Info.Rect.Bottom = drawinfo.VerticalStart + h;
            drawinfo.LastVerticalPosition = drawinfo.VerticalStart;
        }
    }
    public void Draw(PdfDocumentItem doc, PdfDrawingInformation drawinfo)
    {
        if(Info is null) throw new Exception("calculate parameters before adjusting and drawing");
        if(!TestPointCount()) throw new Exception("geometry doesnt have required amount of points");
        DrawGeometry(doc.GetCurrentXGraphics(), drawinfo.Pen, drawinfo.Brush);
        if(Mode == ItemMode.Dynamic) drawinfo.LastVerticalPosition = Info.Rect.Bottom;
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
}
