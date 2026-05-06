namespace TaskApi.Models.Dtos
{
    public class ChangeCredentialsDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string? NewUsername { get; set; }
        public string? NewPassword { get; set; }
    }
}
