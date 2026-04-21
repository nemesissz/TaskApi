using System.ComponentModel.DataAnnotations;

public class AddCommentDto
{
    [Required]
    public string Text { get; set; } = string.Empty;
}
