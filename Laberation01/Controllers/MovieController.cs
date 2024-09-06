using Laberation01.Application.Common.Utiliy;
using Laberation01.Domain.Entities;
using Laberation01.Infrastructure.Data;
using Laberation01.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Laberation01.Controllers;

public class MovieController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public MovieController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
    }

    public IActionResult Index(string? OrderBy = null, string? SearchQuery = null)
    {
        // Retrieve session values if the parameters are empty
        if (OrderBy is null)
{
            OrderBy = HttpContext.Session.GetString("OrderBy") ?? "";
        }
        else
        {
            HttpContext.Session.SetString("OrderBy", OrderBy);
        }

        if (SearchQuery is null)
        {
            SearchQuery = HttpContext.Session.GetString("SearchQuery") ?? "";
        }
        else
        {
            HttpContext.Session.SetString("SearchQuery", SearchQuery);
        }

        // Get movies from the database as IQueryable and include the related actors
        var movies = _db.Movies.Include(m => m.Actors).AsQueryable();

        // Apply search query directly in the database query
        if (!string.IsNullOrEmpty(SearchQuery))
        {
            movies = movies.Where(m => m.Title.Contains(SearchQuery)
                                    || (m.Description != null && m.Description.Contains(SearchQuery)));
        }

        // Apply sorting in the database
        switch (OrderBy.ToLower())
        {
            case SD.SortByReleaseDate:
                movies = movies.OrderBy(m => m.ReleaseDate);
                ViewData[SD.SortBy] = SD.SortByReleaseDate;
                break;
            case SD.SortByRunningTime:
                movies = movies.OrderBy(m => m.RunningTime);
                ViewData[SD.SortBy] = SD.SortByTitle;
                break;
            default:
                movies = movies.OrderBy(m => m.Title);
                ViewData[SD.SortBy] = SD.SortByTitle;
                break; 
        }
        ViewBag.SearchQuery = SearchQuery;

        // Convert the query to a list and pass it to the view
        return View(movies.ToList()); // Query executed when ToList() is called
    }

    public IActionResult Create()
    {
        // Populate view with the view model
        var viewModel = new MovieViewModel
        {
            Actors = _db.Actors.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.Name
            }).ToList()
        };
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Create(MovieViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            // Create the movie
            var movie = viewModel.Movie;
            movie.PreviewUrl = SaveVideo(movie.Preview, movie.Title);

            // Handle selected actors
            movie.Actors = _db.Actors
                .Where(a => viewModel.SelectedActorIds.Contains(a.Id))
                .ToList();

            // Save the movie to the database
            _db.Movies.Add(movie);
            await _db.SaveChangesAsync();
            TempData[SD.Success] = SD.SuccessCreateMessage;
            return RedirectToAction(nameof(Index));
        }

        // Repopulate the actor list if validation fails
        viewModel.Actors = _db.Actors.Select(a => new SelectListItem
        {
            Value = a.Id.ToString(),
            Text = a.Name
        }).ToList();
        TempData[SD.Error] = SD.ErrorCreateMessage;
        return View(viewModel);
    }

    public IActionResult Update(int id)
    {
        // Get movie and actors from the database
        var movieFromDb = _db.Movies
                             .Include(m => m.Actors)
                             .FirstOrDefault(m => m.Id == id);

        // Handle invalid id
        if (movieFromDb == null)
        {
            return NotFound();
        }

        // Populate view with the view model
        var viewModel = new MovieViewModel
        {
            Actors = _db.Actors.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.Name
            }).ToList(),
            Movie = movieFromDb,
            SelectedActorIds = movieFromDb.Actors.Select(a => a.Id).ToList()
        };
        return View(viewModel);
    }

    [HttpPost]
    public IActionResult Update(MovieViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            // Fetch the existing movie from the database
            var movieFromDb = _db.Movies
                                 .Include(m => m.Actors) // Include actors to handle relationship
                                 .FirstOrDefault(m => m.Id == viewModel.Movie.Id);

            if (movieFromDb == null)
            {
                TempData[SD.Error] = SD.ErrorUpdateMessage;
                return NotFound();
            }

            // Update the properties of the existing movie
            movieFromDb.Title = viewModel.Movie.Title;
            movieFromDb.Description = viewModel.Movie.Description;
            movieFromDb.ReleaseDate = viewModel.Movie.ReleaseDate;
            movieFromDb.RunningTime = viewModel.Movie.RunningTime;

            if (viewModel.Movie.Preview != null)
            {
                // Delete the old video and save the new one
                DeleteVideo(movieFromDb.PreviewUrl);
                movieFromDb.PreviewUrl = SaveVideo(viewModel.Movie.Preview, movieFromDb.Title);
            }

            // Update the Actors relationship
            movieFromDb.Actors = _db.Actors
                .Where(a => viewModel.SelectedActorIds.Contains(a.Id))
                .ToList();

            // Save changes to the database
            _db.SaveChanges();

            TempData[SD.Success] = SD.SuccessUpdateMessage;
            return RedirectToAction("Index");
        }

        // Repopulate the actor list if validation fails
        viewModel.Actors = _db.Actors.Select(a => new SelectListItem
        {
            Value = a.Id.ToString(),
            Text = a.Name
        }).ToList();
        TempData[SD.Error] = SD.ErrorUpdateMessage;
        return View(viewModel);
    }

    public IActionResult Delete(int id)
    {
        // Get movie and actors from the database
        var movieFromDb = _db.Movies
                             .Include(m => m.Actors)
                             .FirstOrDefault(m => m.Id == id);

        // Handle invalid id
        if (movieFromDb == null)
        {
            return NotFound();
        }

        // Populate view with the view model
        var viewModel = new MovieViewModel
        {
            Actors = _db.Actors.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.Name
            }).ToList(),
            Movie = movieFromDb,
            SelectedActorIds = movieFromDb.Actors.Select(a => a.Id).ToList()
        };
        return View(viewModel);
    }

    [HttpPost]
    public IActionResult Delete(Movie movie)
    {
        // Delete movie from database
        var movieFromDb = _db.Movies.FirstOrDefault(m => m.Id == movie.Id);
        if (movieFromDb == null)
        {
            TempData[SD.Error] = SD.ErrorDeleteMessage;
            return NotFound();
        }
        DeleteVideo(movieFromDb.PreviewUrl);
        _db.Movies.Remove(movieFromDb);
        _db.SaveChanges();
        TempData[SD.Success] = SD.SuccessDeleteMessage;
        return RedirectToAction("Index");
    }



    // Helper method for saving IFormFile to the wwwroot/videos folder
    private string SaveVideo(IFormFile video, string name)
    {
        const string videoFolder = "videos";
        var fileExtension = Path.GetExtension(video.FileName);
        var fileName = $"{name}{fileExtension}";
        var fullFilePath = Path.Combine(_webHostEnvironment.WebRootPath, videoFolder, fileName);
        var relativeFilePath = Path.Combine("/", videoFolder, fileName).Replace("\\", "/");

        using (var fileStream = new FileStream(fullFilePath, FileMode.Create))
        {
            video.CopyTo(fileStream);
        }

        // Return the relative path
        return relativeFilePath;
    }

    // Helper method for removing videos from the wwwroot/videos folder
    private void DeleteVideo(string relativeFilePath)
    {
        // Combine the web root path with the relative path to get the full file system path
        var fullFilePath = Path.Combine(_webHostEnvironment.WebRootPath, relativeFilePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

        if (System.IO.File.Exists(fullFilePath))
        {
            System.IO.File.Delete(fullFilePath);
        }
    }
}
