using Microsoft.EntityFrameworkCore;

namespace Benutzerverwaltungssoftware.Data;

public class CustomerManagementContext : DbContext
{
    #region DBSET Authentification
    public DbSet<UserAccount>? UserAccounts { get; set; }
    #endregion

    #region DBSET CustomerManagement
    public DbSet<Customer>? Customers { get; set; }
    public DbSet<InvoiceItem>? InvoiceItems { get; set; }
    public DbSet<CustomerInvoiceItem>? CustomerInvoiceItems { get; set; }
    public DbSet<CustomerFile>? CustomerFiles { get; set; }
    #endregion

    #region DBSET Logs
    public DbSet<Log>? Logs { get; set; }
    #endregion

    string DbPath;
    public CustomerManagementContext(int year)
    {
        DbPath = $"Database/db{year}.sqlite";

        Database.Migrate();
        SaveChanges();
    }
    public CustomerManagementContext()
    {
        DbPath = $"Database/dbbase.sqlite";
    }

    protected override void OnModelCreating(ModelBuilder mb) => base.OnModelCreating(mb);
    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={DbPath}");
}