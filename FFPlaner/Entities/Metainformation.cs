using System.ComponentModel.DataAnnotations;

namespace FFPlaner.Entities;

public class Metainformation
{
    [Key]
    [Required]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Value { get; set; } = string.Empty;

    [Required]
    public DateTime UpdatedAt { get; set; }
}
