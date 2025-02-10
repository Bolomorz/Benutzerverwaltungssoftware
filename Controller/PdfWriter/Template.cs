namespace Benutzerverwaltungssoftware.Pdf;

internal static class PdfWriter
{
    internal static PdfFile Write(PdfTemplate template) => template.Write();
}

internal abstract class PdfTemplate
{
    protected abstract PdfDocumentItem Document { get; set; }
    protected abstract PdfDrawingInformation Information { get; set; }
    protected abstract List<ITemplateItem> Items { get; set; }
    public PdfFile Write()
    {
        foreach(var Item in Items)
        {
            Item.CalcParameters(Document, Information);
            Item.AdjustNewPage(Document, Information);
            Item.Draw(Document, Information);
        }
        var file = Document.GetFile();
        return file is not null ? new PdfFile(){ Success = true, Data = file } : new PdfFile(){ Success = false };
    }
}