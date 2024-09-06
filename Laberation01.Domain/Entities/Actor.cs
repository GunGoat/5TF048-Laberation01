using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laberation01.Domain.Entities;

public class Actor
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }

    [NotMapped]
    public IFormFile? Preview { get; set; }
    public string? PreviewUrl { get; set; }

    // Navigation property for the many-to-many relationship
    [ValidateNever]
    public ICollection<Movie> Movies { get; set; } = new List<Movie>();
}

