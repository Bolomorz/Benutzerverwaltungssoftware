using Benutzerverwaltungssoftware.Data;

namespace Benutzerverwaltungssoftware.Session;

internal class AuthenticatedUser
{
    internal UserAccount UserAccount { get; private set; }
    internal Role UserRole { get; private set; }

    internal AuthenticatedUser(UserAccount account)
    {
        UserAccount = account;
        UserRole = account.Role is not null ? (Role)account.Role : Role.User;
    }

    internal ReturnDialog SetUserAccount(string username, string password)
    {
        var rd = UserManagementPacker.Authentification.SetUserAccount(UserAccount.UserAccountID, username, password);
        if(!rd.Message.Success) 
        {
            var wl = UserManagementPacker.Logs.WriteLog("SetUserAccount", rd.Message.Error, ObjectType.UA, UserAccount.UserAccountID);
            if(!wl.Message.Success) throw new Exception(wl.Message.Error);
        }
        return rd;
    }
    /// <summary>
    /// CustomerFile.Customer
    /// </summary>
    /// <param name="customerfileID"></param>
    /// <returns></returns>
    internal ReturnDialog<CustomerFile> GetCustomerFile(int customerfileID)
    {
        var rd = UserManagementPacker.CustomerFile.GetOne(customerfileID);
        if(!rd.Message.Success) 
        {
            var wl = UserManagementPacker.Logs.WriteLog("GetCustomerFile", rd.Message.Error, ObjectType.CF, customerfileID);
            if(!wl.Message.Success) throw new Exception(wl.Message.Error);
        }
        return rd;
    }
    internal ReturnDialog SaveCustomerFile(string filename, byte[] data, DateTime time, int customerID, int? customerfileID)
    {
        var rd = UserManagementPacker.CustomerFile.Save(filename, data, time, customerID, customerfileID);
        if(!rd.Message.Success) 
        {
            var wl = UserManagementPacker.Logs.WriteLog("SaveCustomerFile", rd.Message.Error, ObjectType.CF, customerfileID);
            if(!wl.Message.Success) throw new Exception(wl.Message.Error);
        }
        return rd;
    }
    internal ReturnDialog DeleteCustomerFile(int customerfileID)
    {
        var rd = UserManagementPacker.CustomerFile.Delete(customerfileID);
        if(!rd.Message.Success) 
        {
            var wl = UserManagementPacker.Logs.WriteLog("DeleteCustomerFile", rd.Message.Error, ObjectType.CF, customerfileID);
            if(!wl.Message.Success) throw new Exception(wl.Message.Error);
        }
        return rd;
    }
    /// <summary>
    /// InvoiceItems.CustomerInvoiceItems
    /// </summary>
    /// <returns></returns>
    internal ReturnDialog<List<InvoiceItem>> GetInvoiceItems()
    {
        var rd = UserManagementPacker.InvoiceItem.GetAll();
        if(!rd.Message.Success) 
        {
            var wl = UserManagementPacker.Logs.WriteLog("GetInvoiceItems", rd.Message.Error, ObjectType.II, null);
            if(!wl.Message.Success) throw new Exception(wl.Message.Error);
        }
        return rd;
    }
    /// <summary>
    /// InvoiceItem.CustomerInvoiceItems.Customer
    /// </summary>
    /// <param name="invoiceitemID"></param>
    /// <returns></returns>
    internal ReturnDialog<InvoiceItem> GetInvoiceItem(int invoiceitemID)
    {
        var rd = UserManagementPacker.InvoiceItem.GetOne(invoiceitemID);
        if(!rd.Message.Success) 
        {
            var wl = UserManagementPacker.Logs.WriteLog("GetInvoiceItem", rd.Message.Error, ObjectType.II, invoiceitemID);
            if(!wl.Message.Success) throw new Exception(wl.Message.Error);
        }
        return rd;
    }
    internal ReturnDialog SaveInvoiceItem(string name, string description, decimal defaultvalue, string transformformula, int? invoiceitemID)
    {
        var rd = UserManagementPacker.InvoiceItem.Save(name, description, defaultvalue, transformformula, invoiceitemID);
        if(!rd.Message.Success) 
        {
            var wl = UserManagementPacker.Logs.WriteLog("SaveInvoiceItem", rd.Message.Error, ObjectType.II, invoiceitemID);
            if(!wl.Message.Success) throw new Exception(wl.Message.Error);
        }
        return rd;
    }
    internal ReturnDialog DeleteInvoiceItem(int invoiceitemID)
    {
        var rd = UserManagementPacker.InvoiceItem.Delete(invoiceitemID);
        if(!rd.Message.Success) 
        {
            var wl = UserManagementPacker.Logs.WriteLog("DeleteInvoiceItem", rd.Message.Error, ObjectType.II, invoiceitemID);
            if(!wl.Message.Success) throw new Exception(wl.Message.Error);
        }
        return rd;
    }
    /// <summary>
    /// Customers.CustomerInvoiceItems
    /// </summary>
    /// <returns></returns>
    internal ReturnDialog<List<Customer>> GetCustomers()
    {
        var rd = UserManagementPacker.Customer.GetAll();
        if(!rd.Message.Success) 
        {
            var wl = UserManagementPacker.Logs.WriteLog("GetCustomers", rd.Message.Error, ObjectType.C, null);
            if(!wl.Message.Success) throw new Exception(wl.Message.Error);
        }
        return rd;
    }
    /// <summary>
    /// Customer.CustomerFiles | Customer.CustomerInvoiceItems.InvoiceItem | Customer.CustomerBookings
    /// </summary>
    /// <param name="customerID"></param>
    /// <returns></returns>
    internal ReturnDialog<Customer> GetCustomer(int customerID)
    {
        var rd = UserManagementPacker.Customer.GetOne(customerID);
        if(!rd.Message.Success) 
        {
            var wl = UserManagementPacker.Logs.WriteLog("GetCustomer", rd.Message.Error, ObjectType.C, customerID);
            if(!wl.Message.Success) throw new Exception(wl.Message.Error);
        }
        return rd;
    }
    internal ReturnDialog SaveCustomer(string givenname, string surname, string street, string streetnumber, string postalcode, string city,
        DateOnly birthday, DateOnly joindate, int? customerID)
    {
        var rd = UserManagementPacker.Customer.Save(givenname, surname, street, streetnumber, postalcode, city, birthday, joindate, customerID);
        if(!rd.Message.Success) 
        {
            var wl = UserManagementPacker.Logs.WriteLog("SaveCustomer", rd.Message.Error, ObjectType.C, customerID);
            if(!wl.Message.Success) throw new Exception(wl.Message.Error);
        }
        return rd;
    }
    internal ReturnDialog BookCustomer(decimal amount, string description, int customerID)
    {
        var rd = UserManagementPacker.Customer.Book(amount, description, customerID);
        if(!rd.Message.Success) 
        {
            var wl = UserManagementPacker.Logs.WriteLog("BookCustomer", rd.Message.Error, ObjectType.C, customerID);
            if(!wl.Message.Success) throw new Exception(wl.Message.Error);
        }
        return rd;
    }
    internal ReturnDialog DeleteCustomer(int customerID)
    {
        var rd = UserManagementPacker.Customer.Delete(customerID);
        if(!rd.Message.Success) 
        {
            var wl = UserManagementPacker.Logs.WriteLog("DeleteCustomer", rd.Message.Error, ObjectType.C, customerID);
            if(!wl.Message.Success) throw new Exception(wl.Message.Error);
        }
        return rd;
    }
    internal ReturnDialog SaveCustomerInvoiceItem(decimal value, bool active, int customerID, int invoiceitemID)
    {
        var rd = UserManagementPacker.CustomerInvoiceItem.Save(value, active, customerID, invoiceitemID);
        if(!rd.Message.Success) 
        {
            var wl = UserManagementPacker.Logs.WriteLog("SaveCustomerInvoiceItem", rd.Message.Error, ObjectType.CII, customerID);
            if(!wl.Message.Success) throw new Exception(wl.Message.Error);
        }
        return rd;
    }
}