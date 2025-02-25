namespace Benutzerverwaltungssoftware.Pages.Management;

public class CustomerModel
{
    public required string GivenName { get; set; }
    public required string Surname { get; set; }
    public required string Street { get; set; }
    public required string StreetNumber { get; set; }
    public required string PostalCode { get; set; }
    public required string City { get; set; }
    public required string Birthday { get; set; }
    public required string JoinDate { get; set; }
    public required decimal PaidAmount { get; set; }
}
public class InvoiceItemModel
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal DefaultValue { get; set; }
    public required string TransformFormula { get; set; }
}