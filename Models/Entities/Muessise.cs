public class Muessise
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Ad { get; set; } = string.Empty;
    public string AdminUsername { get; set; } = string.Empty;
    public DateTime YaranmaTarixi { get; set; } = DateTime.UtcNow;

    public ICollection<AppUser> Users { get; set; } = [];
    public ICollection<Bolme> Bolmeler { get; set; } = [];
}
