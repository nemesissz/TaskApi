public class Bolme
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Ad { get; set; } = string.Empty;
    public Guid MuessiseId { get; set; }
    public string? AdminUsername { get; set; }

    public Muessise? Muessise { get; set; }
    public ICollection<AppUser> Users { get; set; } = [];
}
