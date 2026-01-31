namespace ServerApp.Models;

public sealed class TodoCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class TodoItem
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Title { get; set; } = "";
    public bool IsDone { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class CreateCategoryRequest
{
    public string? Name { get; set; }
}

public sealed class CreateTodoRequest
{
    public int CategoryId { get; set; }
    public string? Title { get; set; }
}

public sealed class UpdateTodoRequest
{
    public string? Title { get; set; }
    public bool IsDone { get; set; }
}
