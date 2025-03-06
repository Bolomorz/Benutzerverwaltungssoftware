using Benutzerverwaltungssoftware.Data;
using Benutzerverwaltungssoftware.Pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Storage;

namespace Benutzerverwaltungssoftware.Pages.Management;

public class ManagementModel : PageModel
{
    [BindProperty] public CustomerModel CDM { get; set; }
    [BindProperty] public InvoiceItemModel IIDM { get; set; }
    [BindProperty] public List<CustomerInvoiceItemModel> CIIDM { get; set; }

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
            Book = "",
            BookingDescription = ""
        };
        IIDM = new()
        {
            Name = "",
            Description = "",
            DefaultValue = "",
            TransformFormula = ""
        };
        CIIDM = new();
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
    public IActionResult OnPostSelectCustomerData()
    {
        Information.InvoiceItemID = null;
        Information.Partial = Management.Partial.CustomerData;
        return Page();
    }
    public IActionResult OnPostSelectCustomerFile()
    {
        Information.InvoiceItemID = null;
        Information.Partial = Management.Partial.CustomerFile;
        return Page();
    }
    public IActionResult OnPostSelectCustomerInvoiceItem()
    {
        Information.InvoiceItemID = null;
        Information.Partial = Management.Partial.CustomerInvoiceItem;
        return Page();
    }
    public IActionResult OnPostSelectCustomerBooking()
    {
        Information.InvoiceItemID = null;
        Information.Partial = Management.Partial.CustomerBooking;
        return Page();
    }
    public IActionResult OnPostSelectInvoiceItemData()
    {
        Information.CustomerID = null;
        Information.Partial = Management.Partial.InvoiceItemData;
        return Page();
    }
    public IActionResult OnPostSelectInvoiceItemCustomer()
    {
        Information.CustomerID = null;
        Information.Partial = Management.Partial.InvoiceItemCustomer;
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
        Information.Partial = Management.Partial.InvoiceItemData;
        return Page();
    }
    
    public IActionResult OnPostPickCustomer(int? id)
    {
        Information.CustomerID = id;
        Information.InvoiceItemID = null;
        Information.Partial = Management.Partial.CustomerData;
        return Page();
    }
    public IActionResult OnPostPickInvoiceItem(int? id)
    {
        Information.CustomerID = null;
        Information.InvoiceItemID = id;
        Information.Partial = Management.Partial.InvoiceItemData;
        return Page();
    }
    
    public IActionResult OnPostBookCustomer()
    {
        if(Global.Session is null || Global.Session.User is null) return RedirectToPage("/Account/Login");

        var rdv = CDM.ValidateBooking();
        if(!rdv.Message.Success)
        {
            Console.WriteLine(rdv.Message.Error);
            Information.Message = rdv.Message;
            return Page();
        }

        if(Information.CustomerID is null || Information.CustomerID == 0) return Page();

        var rdb = Global.Session.User.BookCustomer(DataModelValidation.StringToDecimal(CDM.Book), CDM.BookingDescription, (int)Information.CustomerID);
        Information.Message = rdb.Message;

        Information.Partial = Management.Partial.CustomerBooking;

        return Page();
    }
    public IActionResult OnPostSaveCustomer()
    {
        if(Global.Session is null || Global.Session.User is null) return RedirectToPage("/Account/Login");

        var rdv = CDM.ValidateSaving();
        if(!rdv.Message.Success)
        {
            Information.Message = rdv.Message;
            return Page();
        }

        if(Information.CustomerID is null) return Page();

        var rds = Information.CustomerID == 0 ? 
            Global.Session.User.SaveCustomer(CDM.GivenName, CDM.Surname, CDM.Street, CDM.StreetNumber, CDM.PostalCode, CDM.City, 
            DataModelValidation.StringToDate(CDM.Birthday), DataModelValidation.StringToDate(CDM.JoinDate), null) :
            Global.Session.User.SaveCustomer(CDM.GivenName, CDM.Surname, CDM.Street, CDM.StreetNumber, CDM.PostalCode, CDM.City, 
            DataModelValidation.StringToDate(CDM.Birthday), DataModelValidation.StringToDate(CDM.JoinDate), Information.CustomerID);
        Information.Message = rds.Message;

        Information.Partial = rds.Message.Success ? Management.Partial.CustomerList : Management.Partial.CustomerData;

        return Page();
    }
    public IActionResult OnPostSaveInvoiceItem()
    {
        if(Global.Session is null || Global.Session.User is null) return RedirectToPage("/Account/Login");

        var rdv = IIDM.Validate();
        if(!rdv.Message.Success)
        {
            Information.Message = rdv.Message;
            return Page();
        }

        if(Information.InvoiceItemID is null) return Page();

        var rds = Information.InvoiceItemID == 0 ?
            Global.Session.User.SaveInvoiceItem(IIDM.Name, IIDM.Description, DataModelValidation.StringToDecimal(IIDM.DefaultValue), IIDM.TransformFormula, null) :
            Global.Session.User.SaveInvoiceItem(IIDM.Name, IIDM.Description, DataModelValidation.StringToDecimal(IIDM.DefaultValue), IIDM.TransformFormula, Information.InvoiceItemID);
        Information.Message = rds.Message;

        Information.Partial = rds.Message.Success ? Management.Partial.InvoiceItemList : Management.Partial.InvoiceItemData;

        return Page();
    }
    public IActionResult OnPostSaveItems(string[] Selected)
    {
        if(Global.Session is null || Global.Session.User is null) return RedirectToPage("/Account/Login");

        if(Information.InvoiceItemID is null && Information.CustomerID is null) return Page();

        foreach(var item in CIIDM) 
        {
            var rdv = item.Validate();
            if(!rdv.Message.Success) 
            { 
                Information.Message = rdv.Message; 
                return Page();
            }
        }

        foreach(var item in CIIDM)
        {
            var selected = IsSelected(Selected, item);
            var rds = Information.CustomerID is not null ? 
                Global.Session.User.SaveCustomerInvoiceItem(DataModelValidation.StringToDecimal(item.Value), selected, (int)Information.CustomerID, item.ID) :
                Information.InvoiceItemID is not null ?
                Global.Session.User.SaveCustomerInvoiceItem(DataModelValidation.StringToDecimal(item.Value), selected, item.ID, (int)Information.InvoiceItemID) :
                new(new(MID.NullValue, false, ""));
            Information.Message = rds.Message.Success ? Information.Message : rds.Message;
        }

        return Page();
    }
    private static bool IsSelected(string[] selected, CustomerInvoiceItemModel model)
    {
        foreach(var select in selected) if(model.ID.ToString() == select) return true;
        return false;
    }

    public IActionResult OnPostDeleteCustomer(int? id)
    {
        if(Global.Session is null || Global.Session.User is null) return RedirectToPage("/Account/Login");

        if(id is null || id < 1) return Page();

        var rdd = Global.Session.User.DeleteCustomer((int)id);
        Information.Message = rdd.Message;

        Information.Partial = Management.Partial.CustomerList;

        return Page();
    }
    public IActionResult OnPostDeleteInvoiceItem(int? id)
    {
        if(Global.Session is null || Global.Session.User is null) return RedirectToPage("/Account/Login");

        if(id is null || id < 1) return Page();

        var rdd = Global.Session.User.DeleteInvoiceItem((int)id);
        Information.Message = rdd.Message;

        Information.Partial = Management.Partial.InvoiceItemList;

        return Page();
    }
    public IActionResult OnPostDeleteCustomerFile(int? id)
    {
        if(Global.Session is null || Global.Session.User is null) return RedirectToPage("/Account/Login");

        if(id is null || id < 1) return Page();

        var rdd = Global.Session.User.DeleteCustomerFile((int)id);
        Information.Message = rdd.Message;

        Information.Partial = Management.Partial.CustomerFile;

        return Page();
    }

    public IActionResult OnPostOpenFile(int? id)
    {
        if(Global.Session is null || Global.Session.User is null) return RedirectToPage("/Account/Login");

        if(id is null || id < 1) return Page();

        var rdo = Global.Session.User.GetCustomerFile((int)id);
        Information.Message = rdo.Message;

        if(rdo.ReturnValue is null) return Page();
        
        PdfFile.OpenDocument(rdo.ReturnValue.Data);

        return Page();
    }
}