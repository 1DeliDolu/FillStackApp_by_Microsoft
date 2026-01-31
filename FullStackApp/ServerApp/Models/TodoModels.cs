namespace ServerApp.Models;

public sealed class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public bool IsDone { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class CreateTodoRequest
{
    public string? Title { get; set; }
}

public sealed class UpdateTodoRequest
{
    public string? Title { get; set; }
    public bool IsDone { get; set; }
}
