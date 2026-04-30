public class Note
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserLogin { get; set; } = string.Empty;
    public string Metn { get; set; } = string.Empty;
    public string Notlar { get; set; } = string.Empty;
    public bool Tamamlanib { get; set; } = false;
    public DateTime YaranmaTarixi { get; set; } = DateTime.UtcNow;
    public bool TarixAktiv { get; set; } = false;
    public bool SaatAktiv { get; set; } = false;
    public string? Tarix { get; set; }
    public string? Saat { get; set; }
}
