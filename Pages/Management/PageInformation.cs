using Benutzerverwaltungssoftware.Data;

namespace Benutzerverwaltungssoftware.Pages.Management;

internal static class Information
{
    internal static int? CustomerID { get; set; }
    internal static int? InvoiceItemID { get; set; }
    internal static Message? Message { get; set; }
    internal static Partial Partial { get; set; } = Partial.CustomerList;
}

internal enum Partial { CustomerData, CustomerFile, CustomerInvoiceItem, CustomerBooking, CustomerList, InvoiceItem, InvoiceItemList}