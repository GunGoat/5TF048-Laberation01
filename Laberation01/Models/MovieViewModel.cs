using Laberation01.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Laberation01.Models;

public class MovieViewModel
{
    public Movie Movie { get; set; }

    // List of all available actors
    public List<SelectListItem> Actors { get; set; } = new List<SelectListItem>();

    // List of selected actor IDs
    public List<int> SelectedActorIds { get; set; } = new List<int>();
}
