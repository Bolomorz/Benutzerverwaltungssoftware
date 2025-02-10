using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Benutzerverwaltungssoftware.Data;

public class UserAccount
{
    [Key][DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int UAID { get; set; }
    public string? Name { get; set; }
    public string? PasswordHash { get; set; }
    public string? HashParameter { get; set; }
    public Role? Role { get; set; }
}
public enum Role { User, Admin }