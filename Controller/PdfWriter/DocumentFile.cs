using System.Runtime.InteropServices;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Benutzerverwaltungssoftware.Pdf;

internal class PdfFile
{
    internal required bool Success { get; set; }
    internal byte[]? Data { get; set; }
    private const string FilePathLinux = "Resources/Temp.pdf";
    private const string FilePathWindows = @"Resources\Temp.pdf";

    internal void OpenDocument()
    {
        if(Data is not null)
        {
            PdfDocument document;
            using(MemoryStream stream = new(Data))
            {
                document = PdfReader.Open(stream);
                string path = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? FilePathWindows : FilePathLinux;
                document.Save(path);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path){ UseShellExecute = true });
            }
        }
    }

    internal static void OpenDocument(byte[]? data)
    {
        var file = new PdfFile(){ Success = true, Data = data };
        file.OpenDocument();
    }
}

internal class DocumentItem
{
    private PdfDocument Document;
    private List<PdfPage> Pages;
    private List<XGraphics> Gfxs;
    private int Index;
    private PageSize PageSize;
    private PageOrientation PageOrientation;
    private FileType Type;
    private string Title;
    private string? PageLayoutLinux;
    private string? PageLayoutWindows;

    internal DocumentItem(string title, FileType type, PageOrientation orientation, string? pagelayoutpng)
    {
        Title = title;
        PageLayoutLinux = pagelayoutpng is not null ? $"Resources/{pagelayoutpng}" : null;
        PageLayoutWindows = pagelayoutpng is not null ? $@"Resources\{pagelayoutpng}" : null;
        Type = type;
        PageSize = PageSize.A4;
        PageOrientation = orientation;
        Index = 0;
        Document = new();
        Document.Info.Title = Title;
        var firstpage = Document.AddPage();
        firstpage.Size = PageSize;
        firstpage.Orientation = PageOrientation;
        var firstgfx = XGraphics.FromPdfPage(firstpage);
        Pages = new() {firstpage};
        Gfxs = new() {firstgfx};
        DrawLayout(firstgfx);
    }

    internal void AddPage()
    {
        var page = Document.AddPage();
        page.Size = PageSize;
        page.Orientation = PageOrientation;
        var gfx = XGraphics.FromPdfPage(page);
        Pages.Add(page);
        Gfxs.Add(gfx);
        Index++;
        DrawLayout(gfx);
    }
    internal FileType GetCurrentType() => Type;
    internal PdfPage GetCurrentPage() => Pages[Index];
    internal XGraphics GetCurrentXGraphics() => Gfxs[Index];
    internal byte[]? GetFile()
    {
        byte[]? pdf = null;
        using(MemoryStream stream = new())
        {
            Document.Save(stream, false);
            pdf = stream.ToArray();
        }
        return pdf;
    }

    private void DrawLayout(XGraphics gfx)
    {
        if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && PageLayoutLinux is not null) DrawImage(gfx, PageLayoutLinux);
        else if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && PageLayoutWindows is not null) DrawImage(gfx, PageLayoutWindows);
        DrawTitle(gfx, Title);
        DrawNumber(gfx, (Index + 1).ToString());
    }
    private void DrawImage(XGraphics gfx, string file)
    {
        XImage img = XImage.FromFile(file);
        XRect rect = new(new XPoint(0,0), 
            PageOrientation == PageOrientation.Portrait ? new XPoint(Settings.CPortrait.A4Width, Settings.CPortrait.A4Height) : new XPoint(Settings.CLandscape.A4Width, Settings.CLandscape.A4Height));
        gfx.DrawImage(img, rect);
    }
    private void DrawTitle(XGraphics gfx, string title)
    {
        XRect rect = PageOrientation is PageOrientation.Portrait ? 
        new(new XPoint(Settings.PageLayoutPortrait.HorizontalStart, Settings.CPortrait.A4Height - 15), new XPoint(Settings.PageLayoutPortrait.HorizontalEnd, Settings.CPortrait.A4Height - 5)) :
        new(new XPoint(Settings.PageLayoutLandscape.HorizontalStart, Settings.CLandscape.A4Height - 15), new XPoint(Settings.PageLayoutLandscape.HorizontalEnd, Settings.CLandscape.A4Height - 5));
        XStringFormat format = new(){LineAlignment = XLineAlignment.Center, Alignment = XStringAlignment.Near};
        XFont font = new XFont("Arial", 5);
        gfx.DrawString(title??" ", font, XBrushes.Black, rect, format);
    }
    private void DrawNumber(XGraphics gfx, string nr)
    {
        XRect rect = PageOrientation is PageOrientation.Portrait ?
        new(new XPoint(Settings.CPortrait.A4Width - 50, Settings.CPortrait.A4Height - 50), new XPoint(Settings.CPortrait.A4Width, Settings.CPortrait.A4Height)) :
        new(new XPoint(Settings.CLandscape.A4Width - 50, Settings.CLandscape.A4Height - 50), new XPoint(Settings.CLandscape.A4Width, Settings.CLandscape.A4Height));
        XStringFormat format = new(){LineAlignment = XLineAlignment.Center, Alignment = XStringAlignment.Center};
        XFont font = new XFont("Arial", 30);
        gfx.DrawString(nr, font, XBrushes.Black, rect, format);
    }
}