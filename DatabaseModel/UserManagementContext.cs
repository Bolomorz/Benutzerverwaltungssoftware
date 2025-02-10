using Microsoft.EntityFrameworkCore;

namespace Benutzerverwaltungssoftware.Data;

public class UserManagementContext : DbContext
{
    public DbSet<UserAccount>? UserAccounts { get; set; }

    string dbpath;
    public UserManagementContext(int year)
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        dbpath = Path.Join(path, $"db{year}.db");

        Database.EnsureCreated();
        SaveChanges();
    }

    protected override void OnModelCreating(ModelBuilder mb) => base.OnModelCreating(mb);
    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={dbpath}");
}