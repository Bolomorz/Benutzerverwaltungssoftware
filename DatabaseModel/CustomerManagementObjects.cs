using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Benutzerverwaltungssoftware.Data;

public class UserAccount
{
    [Key][DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int UserAccountID { get; set; }
    public string? Name { get; set; }
    public string? PasswordHash { get; set; }
    public string? HashParameter { get; set; }
    public Role? Role { get; set; }
}
public enum Role { User, Admin }

public class Customer
{
    [Key][DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int CustomerID { get; set; }
    public string? GivenName { get; set; }
    public string? Surname { get; set; }
    public string? Street { get; set; }
    public string? StreetNumer { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public DateTime? Birthday { get; set; }
    public DateTime? JoinDate { get; set; }
    public decimal? PaidAmount { get; set; }

    public List<CustomerInvoiceItem> CustomerInvoiceItems { get; set; } = new();
    public List<CustomerFile> CustomerFiles { get; set;} = new();
}

public class InvoiceItem
{
    [Key][DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int InvoiceItemID { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? DefaultValue { get; set; }
    public string? TransformFormula { get; set; }

    public List<CustomerInvoiceItem> CustomerInvoiceItems { get; set; } = new();
}

public class CustomerInvoiceItem
{
    [Key][DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int CustomerInvoiceItemID { get; set; }
    public decimal? Value { get; set; }
    public bool? Active { get; set; }

    [ForeignKey("Customer")] public int CustomerID { get; set; }
    public Customer? Customer { get; set; }

    [ForeignKey("InvoiceItem")] public int InvoiceItemID { get; set;}
    public InvoiceItem? InvoiceItem { get; set; }
}

public class CustomerFile
{
    [Key][DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int CustomerFileID { get; set; }
    public string? FileName { get; set; }
    public byte[]? Data { get; set; }
    public DateTime? Time { get; set; }

    [ForeignKey("Customer")] public int CustomerID { get; set; }
    public Customer? Customer { get; set; }
}

public class Log
{
    [Key][DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int LogID { get; set; }
    public DateTime? Time { get; set; }
    public string? Code { get; set; }
    public string? Message { get; set; }
    public ObjectType? Type { get; set; }
    public int? ObjectID { get; set; }
}
public enum ObjectType { C, II, CII, CF, UA}