using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Laberation01.Domain.Entities;

public class Movie
{
    [Key]
    public int Id { get; set; }
    [RegularExpression(@"^[^<>:;,?""*|/\\]+$", ErrorMessage = "The Title contains invalid characters for a filename.")]
    public required string Title { get; set; }
    [Required]
    public string? Description { get; set; }
    public DateTime ReleaseDate { get; set; } = DateTime.Now;
    public TimeSpan RunningTime { get; set; } = TimeSpan.Zero;

    [NotMapped]
    public IFormFile? Preview { get; set; }
    public string? PreviewUrl { get; set; }

    // Navigation property for the many-to-many relationship
    [ValidateNever]
    public ICollection<Actor> Actors { get; set; } = new List<Actor>();
}
