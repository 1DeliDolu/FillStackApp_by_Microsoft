using Microsoft.AspNetCore.Mvc;
using ServerApp.Models;
using System.Collections.Concurrent;

namespace ServerApp.Controllers;

[ApiController]
[Route("api/todos")]
public class TodosController : ControllerBase
{
    internal static readonly ConcurrentDictionary<int, TodoItem> Todos = new();
    private static int _nextTodoId = 0;

    // GET api/todos?categoryId=1
    [HttpGet]
    public ActionResult<IEnumerable<TodoItem>> GetAll([FromQuery] int? categoryId)
    {
        var items = Todos.Values.AsEnumerable();

        if (categoryId.HasValue)
            items = items.Where(t => t.CategoryId == categoryId.Value);

        var result = items
            .OrderByDescending(t => t.Id)
            .ToArray();

        return Ok(result);
    }

    [HttpPost]
    public ActionResult<TodoItem> Create([FromBody] CreateTodoRequest req)
    {
        if (req.CategoryId <= 0)
            return BadRequest(new { message = "CategoryId is required." });

        if (!TodoCategoriesController.Categories.ContainsKey(req.CategoryId))
            return BadRequest(new { message = "Category does not exist." });

        var title = (req.Title ?? "").Trim();
        if (string.IsNullOrWhiteSpace(title))
            return BadRequest(new { message = "Title is required." });

        if (title.Length > 200)
            return BadRequest(new { message = "Title must be <= 200 characters." });

        var id = Interlocked.Increment(ref _nextTodoId);

        var item = new TodoItem
        {
            Id = id,
            CategoryId = req.CategoryId,
            Title = title,
            IsDone = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        Todos[id] = item;
        return Ok(item);
    }

    [HttpPut("{id:int}")]
    public ActionResult<TodoItem> Update(int id, [FromBody] UpdateTodoRequest req)
    {
        if (!Todos.TryGetValue(id, out var existing))
            return NotFound();

        var title = (req.Title ?? "").Trim();
        if (string.IsNullOrWhiteSpace(title))
            return BadRequest(new { message = "Title is required." });

        if (title.Length > 200)
            return BadRequest(new { message = "Title must be <= 200 characters." });

        existing.Title = title;
        existing.IsDone = req.IsDone;

        Todos[id] = existing;
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        return Todos.TryRemove(id, out _) ? NoContent() : NotFound();
    }
}
