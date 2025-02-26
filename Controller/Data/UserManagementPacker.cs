using System.Transactions;
using System.Xml.Schema;
using Benutzerverwaltungssoftware.Security;
using Microsoft.EntityFrameworkCore;

namespace Benutzerverwaltungssoftware.Data;

internal static class UserManagementPacker
{
    internal static ReturnDialog TestDatabaseConnection()
    {
        try
        {
            using var cmc = new CustomerManagementContext(Global.Year);

            return cmc.UserAccounts is not null && cmc.Customers is not null && cmc.CustomerFiles is not null && cmc.CustomerInvoiceItems is not null && cmc.InvoiceItems is not null ? 
            new(Message.ConnectionSuccessful) : new(Message.ConnectionFailed);
        }
        catch(Exception ex) 
        {
            return new(new(MID.ErrorThrown, false, $"Fehler in TestDatabaseConnection:\n\n{ex}"));
        }
    }
    internal static class Logs
    {
        internal static ReturnDialog WriteLog(string code, string message, ObjectType? type, int? oid)
        {
            try
            {
                using var cmc = new CustomerManagementContext(Global.Year);

                if(cmc.Logs is null) return new(Message.ConnectionFailed);

                cmc.Logs.Add(new()
                {
                    Time = DateTime.Now,
                    Code = code,
                    Message = message,
                    Type = type,
                    ObjectID = oid
                });

                return new(Message.OperationSuccessful);
            }
            catch(Exception ex) 
            {
                return new(new(MID.ErrorThrown, false, $"Fehler in WriteLog:\n\n{ex}"));
            }
        }
    }
    internal static class Authentification
    {
        internal static ReturnDialog<UserAccount> Authenticate(string username, string plainpw)
        {
            try
            {
                using var cmc = new CustomerManagementContext(Global.Year);

                if(cmc.UserAccounts is null) return new(Message.ConnectionFailed, null);

                var user = cmc.UserAccounts.FirstOrDefault(x => x.Name == username);

                if(user is null) return new(new(MID.NotFound, false, $"Der Nutzer mit Name {username} wurde nicht gefunden!"), null);

                var auth = Hashing.Authenticate(user, plainpw);

                return auth.Message.Success ? new(Message.OperationSuccessful, user) : new(auth.Message, null);
            }
            catch(Exception ex)
            {
                return new(new(MID.ErrorThrown, false, $"Fehler in Authentication.Authenticate:\n\n{ex}"), null);
            }
        }
        internal static ReturnDialog SetUserAccount(int useraccountID, string username, string password)
        {
            using var cmc = new CustomerManagementContext(Global.Year);
            using var transaction = cmc.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);

            try
            {
                if(cmc.UserAccounts is null) { transaction.Rollback(); return new(Message.ConnectionFailed); }

                var user = cmc.UserAccounts.FirstOrDefault(x => x.UserAccountID == useraccountID);

                if(user is null) { transaction.Rollback(); return new(new(MID.NotFound, false, $"Der Nutzer mit ID {useraccountID} wurde nicht gefunden!")); }

                var data = Hashing.GenerateUserData(username, password);

                if(data.Message.Success && data.ReturnValue is not null)
                {
                    user.Name = username;
                    user.PasswordHash = data.ReturnValue.PasswordHash;
                    user.HashParameter = data.ReturnValue.HashParameter;
                }

                cmc.SaveChanges();

                transaction.Commit();

                return data.Message.Success ? new(Message.OperationSuccessful) : new(data.Message);
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                return new(new(MID.ErrorThrown, false, $"Fehler in Authentication.SetUserAccount: {ex}"));
            }
        }
    }
    internal static class CustomerFile
    {
        /// <summary>
        /// CustomerFile.Customer
        /// </summary>
        /// <param name="customerfileID"></param>
        /// <returns></returns>
        internal static ReturnDialog<Data.CustomerFile> GetOne(int customerfileID)
        {
            try
            {
                using var cmc = new CustomerManagementContext(Global.Year);

                if(cmc.CustomerFiles is null) return new(Message.ConnectionFailed, null);

                var file = cmc.CustomerFiles
                .Include(x => x.Customer)
                .FirstOrDefault(x => x.CustomerFileID == customerfileID);

                return file is not null ? new(Message.OperationSuccessful, file) : new(new(MID.NotFound, false, $"Datei mit ID {customerfileID} wurde nicht gefunden."), null);
            }
            catch(Exception ex)
            {
                return new(new(MID.ErrorThrown, false, $"error in CustomerFile.GetOne: {ex}"), null);
            }
        }
        internal static ReturnDialog Save(string filename, byte[] data, DateTime time, int customerID, int? customerfileID)
        {
            using var cmc = new CustomerManagementContext(Global.Year);
            using var transaction = cmc.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);

            try
            {
                if(cmc.CustomerFiles is null) { transaction.Rollback(); return new(Message.ConnectionFailed); }

                var file = customerfileID is not null ? cmc.CustomerFiles.FirstOrDefault(x => x.CustomerFileID == customerfileID) : null;

                if(customerfileID is null)
                {
                    cmc.CustomerFiles.Add(new()
                    {
                        FileName = filename,
                        Data = data,
                        Time = time,
                        CustomerID = customerID
                    });
                } 
                else if(file is not null)
                {
                    file.FileName = filename;
                    file.Data = data;
                    file.Time = time;
                    file.CustomerID = customerID;
                }

                cmc.SaveChanges();

                transaction.Commit();

                return new(Message.OperationSuccessful);
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                return new(new(MID.ErrorThrown, false, $"error in CustomerFile.Save: {ex}"));
            }
        }
        internal static ReturnDialog Delete(int customerfileID)
        {
            using var cmc = new CustomerManagementContext(Global.Year);
            using var transaction = cmc.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);

            try
            {
                if(cmc.CustomerFiles is null) { transaction.Rollback(); return new(Message.ConnectionFailed); }

                var file = cmc.CustomerFiles.FirstOrDefault(x => x.CustomerFileID == customerfileID);
                if(file is not null) cmc.CustomerFiles.Remove(file);

                cmc.SaveChanges();

                transaction.Commit();

                return new(Message.OperationSuccessful);
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                return new(new(MID.ErrorThrown, false, $"error in CustomerFile.Delete: {ex}"));
            }
        }
    }
    internal static class InvoiceItem
    {
        /// <summary>
        /// InvoiceItems.CustomerInvoiceItems
        /// </summary>
        /// <returns></returns>
        internal static ReturnDialog<List<Data.InvoiceItem>> GetAll()
        {
            try
            {
                using var cmc = new CustomerManagementContext(Global.Year);

                if(cmc.InvoiceItems is null) return new(Message.ConnectionFailed, null);

                var items = cmc.InvoiceItems
                .Include(x => x.CustomerInvoiceItems)
                .ToList();

                return new(Message.OperationSuccessful, items);
            }
            catch(Exception ex)
            {
                return new(new(MID.ErrorThrown, false, $"error in InvoiceItem.GetAll: {ex}"), null);
            }
        }
        /// <summary>
        /// InvoiceItem.CustomerInvoiceItems.Customer
        /// </summary>
        /// <param name="invoiceitemID"></param>
        /// <returns></returns>
        internal static ReturnDialog<Data.InvoiceItem> GetOne(int invoiceitemID)
        {
            try
            {
                using var cmc = new CustomerManagementContext(Global.Year);

                if(cmc.InvoiceItems is null) return new(Message.ConnectionFailed, null);

                var item = cmc.InvoiceItems
                .Include(x => x.CustomerInvoiceItems).ThenInclude(x => x.Customer)
                .FirstOrDefault(x => x.InvoiceItemID == invoiceitemID);

                return item is not null ? new(Message.OperationSuccessful, item) : new(new(MID.NotFound, false, $"Rechnungsposten mit ID {invoiceitemID} wurde nicht gefunden"), null);
            }
            catch(Exception ex)
            {
                return new(new(MID.ErrorThrown, false, $"error in InvoiceItem.GetOne: {ex}"), null);
            }
        }
        internal static ReturnDialog Save(string name, string description, decimal defaultvalue, string transformformula, int? invoiceitemID)
        {
            using var cmc = new CustomerManagementContext(Global.Year);
            using var transaction = cmc.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);

            try
            {
                if(cmc.InvoiceItems is null || cmc.Customers is null || cmc.CustomerInvoiceItems is null) 
                    { transaction.Rollback(); return new(Message.ConnectionFailed); }

                var item = invoiceitemID is not null ? cmc.InvoiceItems.FirstOrDefault(x => x.InvoiceItemID == invoiceitemID) : null;

                if(invoiceitemID is null)
                {
                    item = new()
                    {
                        Name = name,
                        Description = description,
                        DefaultValue = defaultvalue,
                        TransformFormula = transformformula,
                    };
                    cmc.InvoiceItems.Add(item);
                    
                    cmc.SaveChanges();

                    var customers = cmc.Customers.ToList();

                    foreach(var customer in customers) 
                        cmc.CustomerInvoiceItems.Add(new()
                        {
                            Value = defaultvalue, 
                            Active = true, 
                            CustomerID = customer.CustomerID,
                            InvoiceItemID = item.InvoiceItemID
                        });
                } 
                else if(item is not null)
                {
                    item.Name = name;
                    item.Description = description;
                    item.DefaultValue = defaultvalue;
                    item.TransformFormula = transformformula;
                }

                cmc.SaveChanges();

                transaction.Commit();

                return new(Message.OperationSuccessful);
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                return new(new(MID.ErrorThrown, false, $"error in InvoiceItem.Save: {ex}"));
            }
        }
        internal static ReturnDialog Delete(int invoiceitemID)
        {
            using var cmc = new CustomerManagementContext(Global.Year);
            using var transaction = cmc.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);

            try
            {
                if(cmc.InvoiceItems is null) { transaction.Rollback(); return new(Message.ConnectionFailed); }

                var item = cmc.InvoiceItems.FirstOrDefault(x => x.InvoiceItemID == invoiceitemID);
                if(item is not null) cmc.InvoiceItems.Remove(item);

                cmc.SaveChanges();

                transaction.Commit();

                return new(Message.OperationSuccessful);
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                return new(new(MID.ErrorThrown, false, $"error in InvoiceItem.Delete: {ex}"));
            }
        }
    }
    internal static class Customer
    {
        /// <summary>
        /// Customers.CustomerInvoiceItems
        /// </summary>
        /// <returns></returns>
        internal static ReturnDialog<List<Data.Customer>> GetAll()
        {
            try
            {
                using var cmc = new CustomerManagementContext(Global.Year);

                if(cmc.Customers is null || cmc.CustomerInvoiceItems is null || cmc.InvoiceItems is null) return new(Message.ConnectionFailed, null);

                var customers = cmc.Customers
                .Include(x => x.CustomerInvoiceItems).ThenInclude(x => x.InvoiceItem)
                .ToList();

                return new(Message.OperationSuccessful, customers);
            }
            catch(Exception ex)
            {
                return new(new(MID.ErrorThrown, false, $"error in Customer.GetAll: {ex}"), null);
            }
        }
        /// <summary>
        /// Customer.CustomerFiles | Customer.CustomerInvoiceItems.InvoiceItem
        /// </summary>
        /// <param name="customerID"></param>
        /// <returns></returns>
        internal static ReturnDialog<Data.Customer> GetOne(int customerID)
        {
            try
            {
                using var cmc = new CustomerManagementContext(Global.Year);

                if(cmc.Customers is null || cmc.CustomerInvoiceItems is null || cmc.InvoiceItems is null || cmc.CustomerFiles is null) 
                    return new(Message.ConnectionFailed, null);

                var customer = cmc.Customers
                .Include(x => x.CustomerFiles)
                .Include(x => x.CustomerInvoiceItems).ThenInclude(x => x.InvoiceItem)
                .FirstOrDefault(x => x.CustomerID == customerID);

                return customer is not null ? new(Message.OperationSuccessful, customer) : new(new(MID.NotFound, false, $"Kunde mit ID {customerID} wurde nicht gefunden"), null);
            }
            catch(Exception ex)
            {
                return new(new(MID.ErrorThrown, false, $"error in Customer.Get: {ex}"), null);
            }
        }
        internal static ReturnDialog Save(string givenname, string surname, string street, string streetnumber, string postalcode, string city,
            DateTime birthday, DateTime joindate, int? customerID)
        {
            using var cmc = new CustomerManagementContext(Global.Year);
            using var transaction = cmc.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);

            try
            {
                if(cmc.InvoiceItems is null || cmc.Customers is null || cmc.CustomerInvoiceItems is null) 
                    { transaction.Rollback(); return new(Message.ConnectionFailed); }

                var customer = customerID is not null ? cmc.Customers.FirstOrDefault(x => x.CustomerID == customerID) : null;

                if(customerID is null)
                {
                    customer = new()
                    {
                        GivenName = givenname,
                        Surname = surname,
                        Street = street,
                        StreetNumer = streetnumber,
                        PostalCode = postalcode,
                        City = city,
                        Birthday = birthday,
                        JoinDate = joindate,
                        PaidAmount = 0
                    };
                    cmc.Customers.Add(customer);

                    cmc.SaveChanges();

                    var items = cmc.InvoiceItems.ToList();

                    foreach(var item in items)
                        cmc.CustomerInvoiceItems.Add(new()
                        {
                            Value = item.DefaultValue, 
                            Active = true, 
                            CustomerID = customer.CustomerID,
                            InvoiceItemID = item.InvoiceItemID
                        });
                } 
                else if(customer is not null)
                {
                    customer.GivenName = givenname;
                    customer.Surname = surname;
                    customer.Street = street;
                    customer.StreetNumer = streetnumber;
                    customer.PostalCode = postalcode;
                    customer.City = city;
                    customer.Birthday = birthday;
                    customer.JoinDate = joindate;
                }

                cmc.SaveChanges();

                transaction.Commit();

                return new(Message.OperationSuccessful);
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                return new(new(MID.ErrorThrown, false, $"error in Customer.Save: {ex}"));
            }
        }
        internal static ReturnDialog Book(decimal amount, int customerID)
        {
            using var cmc = new CustomerManagementContext(Global.Year);
            using var transaction = cmc.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);

            try
            {
                if(cmc.Customers is null) { transaction.Rollback(); return new(Message.ConnectionFailed); }

                var customer = cmc.Customers.FirstOrDefault(x => x.CustomerID == customerID);

                if(customer is null) { transaction.Rollback(); return new(new(MID.NotFound, false, $"Kunde mit ID {customerID} wurde nicht gefunden"));}

                customer.PaidAmount = customer.PaidAmount is not null ? customer.PaidAmount + amount : amount;

                cmc.SaveChanges();

                transaction.Commit();

                return new(Message.OperationSuccessful);
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                return new(new(MID.ErrorThrown, false, $"error in Customer.Save: {ex}"));
            }
        }
        internal static ReturnDialog Delete(int customerID)
        {
            using var cmc = new CustomerManagementContext(Global.Year);
            using var transaction = cmc.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);

            try
            {
                if(cmc.Customers is null) { transaction.Rollback(); return new(Message.ConnectionFailed); }

                var customer = cmc.Customers.FirstOrDefault(x => x.CustomerID == customerID);
                if(customer is not null) cmc.Customers.Remove(customer);

                cmc.SaveChanges();

                transaction.Commit();

                return new(Message.OperationSuccessful);
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                return new(new(MID.ErrorThrown, false, $"error in Customer.Delete: {ex}"));
            }
        }
    }
    internal static class CustomerInvoiceItem
    {
        internal static ReturnDialog Save(decimal value, bool active, int customerID, int invoiceitemID)
        {
            using var cmc = new CustomerManagementContext(Global.Year);
            using var transaction = cmc.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);

            try
            {
                if(cmc.CustomerInvoiceItems is null) 
                    { transaction.Rollback(); return new(Message.ConnectionFailed); }

                var item = cmc.CustomerInvoiceItems.FirstOrDefault(x => x.CustomerID == customerID && x.InvoiceItemID == invoiceitemID);

                if(item is not null)
                {
                    item.Value = value;
                    item.Active = active;                    
                }

                cmc.SaveChanges();

                transaction.Commit();

                return item is not null ? new(Message.OperationSuccessful) : new(new(MID.NotFound, false, $"Rechnungsposten des Kunden mit [KundenID {customerID}|PostenID {invoiceitemID}] wurde nicht gefunden"));
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                return new(new(MID.ErrorThrown, false, $"error in CustomerInvoiceItem.Save: {ex}"));
            }
        }
    }
}