using Laberation01.Application.Common.Utiliy;
using Laberation01.Domain.Entities;
using Laberation01.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Laberation01.Controllers;

public class ActorController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ActorController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
    }

    public IActionResult Index(string OrderBy = "", string SearchQuery = "")
    {
        // Get movies from the database as IQueryable and include the related actors
        var actors = _db.Actors.AsQueryable();

        // Apply search query directly in the database query
        if (!string.IsNullOrEmpty(SearchQuery))
        {
            actors = actors.Where(a => a.Name.Contains(SearchQuery));
        }

        // Apply sorting in the database
        switch (OrderBy.ToLower())
        {
            case SD.SortByName:
                actors = actors.OrderBy(m => m.Name);
                break;
            default:
                actors = actors.OrderBy(m => m.Id);
                break;
        }

        return View(actors.ToList());
    }


    public IActionResult Create()
    {
        var actor = new Actor() {
            Name = "",
            PreviewUrl = "https://via.placeholder.com/150"
        };
        return View(actor);
    }

    [HttpPost]
    public IActionResult Create(Actor actor)
    {
        if (ModelState.IsValid && actor.Preview != null)
        {
            actor.PreviewUrl = SaveImage(actor.Preview, actor.Name);
            _db.Actors.Add(actor);
            _db.SaveChanges();
            TempData[SD.Success] = SD.SuccessCreateMessage;
            return RedirectToAction(nameof(Index));
        }
        TempData[SD.Error] = SD.ErrorCreateMessage;
        return View(actor);
    }

    public IActionResult Update(int id)
    {
        var actorFromDb = _db.Actors.FirstOrDefault(actor => actor.Id == id);
        if (actorFromDb != null)
        {
            return View(actorFromDb);
        }
        return NotFound();
    }

    [HttpPost]
    public IActionResult Update(Actor actor)
    {
        var id = actor.Id;
        if (_db.Actors.Any(actor => actor.Id == id))
        {
            if (actor.Preview != null)
            {
                DeleteImage(actor.PreviewUrl);
                actor.PreviewUrl = SaveImage(actor.Preview, actor.Name);
            }
            _db.Actors.Update(actor);
            _db.SaveChanges();
            TempData[SD.Success] = SD.SuccessUpdateMessage;
            return RedirectToAction(nameof(Index));
        }
        TempData[SD.Error] = SD.ErrorUpdateMessage;
        return NotFound();
    }

    public IActionResult Delete(int id)
    {
        var actorFromDb = _db.Actors.FirstOrDefault(actor => actor.Id == id);
        if (actorFromDb != null)
        {
            return View(actorFromDb);
        }
        return NotFound();
    }

    [HttpPost]
    public IActionResult Delete(Actor actor)
    {
        var actorFromDb = _db.Actors.FirstOrDefault(actor => actor.Id == actor.Id);
        if (actorFromDb != null)
        {
            DeleteImage(actorFromDb.PreviewUrl);
            _db.Actors.Remove(actorFromDb);
            _db.SaveChanges();
            TempData[SD.Success] = SD.SuccessDeleteMessage;
            return RedirectToAction(nameof(Index));
        }
        TempData[SD.Error] = SD.ErrorDeleteMessage;
        return NotFound();
    }

    // Helper method for saving IFormFile to the wwwroot/images folder
    private string SaveImage(IFormFile image, string name)
    {
        const string imageFolder = "images";
        var fileExtension = Path.GetExtension(image.FileName);
        var fileName = $"{name}{fileExtension}";
        var fullFilePath = Path.Combine(_webHostEnvironment.WebRootPath, imageFolder, fileName);
        var relativeFilePath = Path.Combine("/", imageFolder, fileName).Replace("\\", "/");

        using (var fileStream = new FileStream(fullFilePath, FileMode.Create))
        {
            image.CopyTo(fileStream);
        }

        return relativeFilePath;
    }

    // Helper method for removing images from the wwwroot/images folder
    private void DeleteImage(string relativeFilePath)
    {
        // Combine the web root path with the relative path to get the full file system path
        var fullFilePath = Path.Combine(_webHostEnvironment.WebRootPath, relativeFilePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

        if (System.IO.File.Exists(fullFilePath))
        {
            System.IO.File.Delete(fullFilePath);
        }
    }
}
