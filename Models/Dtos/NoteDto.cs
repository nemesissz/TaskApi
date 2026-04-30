public class NoteDto
{
    public Guid Id { get; set; }
    public string Metn { get; set; } = string.Empty;
    public string Notlar { get; set; } = string.Empty;
    public bool Tamamlanib { get; set; }
    public string YaranmaTarixi { get; set; } = string.Empty;
    public bool TarixAktiv { get; set; }
    public bool SaatAktiv { get; set; }
    public string? Tarix { get; set; }
    public string? Saat { get; set; }
}

public class SaveNoteDto
{
    public string Metn { get; set; } = string.Empty;
    public string Notlar { get; set; } = string.Empty;
    public bool Tamamlanib { get; set; }
    public bool TarixAktiv { get; set; }
    public bool SaatAktiv { get; set; }
    public string? Tarix { get; set; }
    public string? Saat { get; set; }
}
