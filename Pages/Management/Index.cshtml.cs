using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Benutzerverwaltungssoftware.Pages.Management;

public class ManagementModel : PageModel
{
    [BindProperty] public CustomerModel CDM { get; set; }
    [BindProperty] public InvoiceItemModel IIDM { get; set; }

    public ManagementModel()
    {
        CDM = new()
        {
            GivenName = "",
            Surname = "",
            Street = "",
            StreetNumber = "",
            PostalCode = "",
            City = "",
            Birthday = "",
            JoinDate = "",
            PaidAmount = 0
        };
        IIDM = new()
        {
            Name = "",
            Description = "",
            DefaultValue = 0,
            TransformFormula = ""
        };
    }

    public IActionResult OnGet() => Global.Session is null || Global.Session.User is null ? RedirectToPage("/PageSession/Login") : Page();

    public IActionResult OnPostSelectCustomers()
    {
        Information.CustomerID = null;
        Information.InvoiceItemID = null;
        Information.Partial = Management.Partial.CustomerList;
        return Page();
    }
    public IActionResult OnPostSelectInvoiceItems()
    {
        Information.CustomerID = null;
        Information.InvoiceItemID = null;
        Information.Partial = Management.Partial.InvoiceItemList;
        return Page();
    }
    public IActionResult OnPostNewCustomer()
    {
        Information.CustomerID = 0;
        Information.InvoiceItemID = null;
        Information.Partial = Management.Partial.CustomerData;
        return Page();
    }
    public IActionResult OnPostNewInvoiceItem()
    {
        Information.CustomerID = null;
        Information.InvoiceItemID = 0;
        Information.Partial = Management.Partial.InvoiceItem;
        return Page();
    }
    public IActionResult OnPostSelectCustomerData()
    {
        Information.Partial = Management.Partial.CustomerData;
        return Page();
    }
    public IActionResult OnPostSelectCustomerFile()
    {
        Information.Partial = Management.Partial.CustomerFile;
        return Page();
    }
    public IActionResult OnPostSelectCustomerInvoiceItem()
    {
        Information.Partial = Management.Partial.CustomerInvoiceItem;
        return Page();
    }
}