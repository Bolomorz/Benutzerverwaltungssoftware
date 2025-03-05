namespace Benutzerverwaltungssoftware.Data;

internal static class CalculateData
{
    internal static decimal SumCIIList(List<CustomerInvoiceItem> items)
    {
        decimal sum = 0;
        foreach(var item in items) sum += item.Value is not null && item.Active is not null && (bool)item.Active ? (decimal)item.Value : 0;
        return sum;
    }
    internal static decimal MissingAmount(Customer customer) => customer.PaidAmount is not null ? SumCIIList(customer.CustomerInvoiceItems) - (decimal)customer.PaidAmount : 0;
}