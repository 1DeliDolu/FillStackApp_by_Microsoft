using Microsoft.AspNetCore.Mvc;
using ServerApp.Models;
using System.Collections.Concurrent;

namespace ServerApp.Controllers;

[ApiController]
[Route("api/todo-categories")]
public class TodoCategoriesController : ControllerBase
{
    // In-memory store (demo)
    internal static readonly ConcurrentDictionary<int, TodoCategory> Categories = new();
    internal static int NextCategoryId = 0;

    [HttpGet]
    public ActionResult<IEnumerable<TodoCategory>> GetAll()
    {
        var data = Categories.Values
            .OrderByDescending(c => c.Id)
            .ToArray();

        return Ok(data);
    }

    [HttpPost]
    public ActionResult<TodoCategory> Create([FromBody] CreateCategoryRequest req)
    {
        var name = (req.Name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { message = "Category name is required." });

        if (name.Length > 100)
            return BadRequest(new { message = "Category name must be <= 100 characters." });

        var id = Interlocked.Increment(ref NextCategoryId);

        var cat = new TodoCategory
        {
            Id = id,
            Name = name,
            CreatedAt = DateTimeOffset.UtcNow
        };

        Categories[id] = cat;
        return CreatedAtAction(nameof(GetAll), new { id }, cat);
    }

    // Category delete: return 409 if it has todos (safe behavior)
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        if (TodosController.Todos.Values.Any(t => t.CategoryId == id))
            return Conflict(new { message = "Category has todos. Delete todos first." });

        return Categories.TryRemove(id, out _) ? NoContent() : NotFound();
    }
}
