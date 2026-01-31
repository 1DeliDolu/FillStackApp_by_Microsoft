
Tamam — `/todo` sayfasını **gerçek bir Todo uygulamasına** çevirelim:

* ✅ **Model** (TodoItem + request modelleri)
* ✅ **Backend Controller** (GET/POST/PUT/DELETE)
* ✅ Frontend (Blazor) **list / add / edit / delete / done**
* ✅ **JS ile form kontrolü** (boş / whitespace engelle) + HTML `required`
* ✅ Yeni kayıt **AJAX ile backend’e gitsin** ve **sayfa yenilenmeden** ekranda görünsün (Blazor’da bu zaten AJAX + state update; full refresh yok)

Aşağıdaki adımları sırayla uygula.

---

# 1) Backend: Todo Controller + Model

## 1.1 Program.cs: Controllers’ı aç

`ServerApp/Program.cs` içine (builder kısmına) şunu ekle:

```csharp
builder.Services.AddControllers();
```

Ve `app` oluşturduktan sonra, endpoint mapping’e ekle:

```csharp
app.MapControllers();
```

> Mevcut Minimal API endpoint’lerin (`/api/productlist`, weather vs.) aynen kalabilir.

---

## 1.2 Model dosyası oluştur

`ServerApp/Models/TodoModels.cs` oluştur:

```csharp
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
```

---

## 1.3 Controller oluştur

`ServerApp/Controllers/TodosController.cs` oluştur:

```csharp
using Microsoft.AspNetCore.Mvc;
using ServerApp.Models;
using System.Collections.Concurrent;

namespace ServerApp.Controllers;

[ApiController]
[Route("api/todos")]
public class TodosController : ControllerBase
{
    private static readonly ConcurrentDictionary<int, TodoItem> Store = new();
    private static int _nextId = 0;

    [HttpGet]
    public ActionResult<IEnumerable<TodoItem>> GetAll()
    {
        var items = Store.Values
            .OrderByDescending(t => t.Id)
            .ToArray();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public ActionResult<TodoItem> GetById(int id)
    {
        return Store.TryGetValue(id, out var item) ? Ok(item) : NotFound();
    }

    [HttpPost]
    public ActionResult<TodoItem> Create([FromBody] CreateTodoRequest req)
    {
        var title = (req.Title ?? "").Trim();
        if (string.IsNullOrWhiteSpace(title))
            return BadRequest(new { message = "Title is required." });

        if (title.Length > 200)
            return BadRequest(new { message = "Title must be <= 200 characters." });

        var id = Interlocked.Increment(ref _nextId);

        var item = new TodoItem
        {
            Id = id,
            Title = title,
            IsDone = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        Store[id] = item;

        return CreatedAtAction(nameof(GetById), new { id }, item);
    }

    [HttpPut("{id:int}")]
    public ActionResult<TodoItem> Update(int id, [FromBody] UpdateTodoRequest req)
    {
        if (!Store.TryGetValue(id, out var existing))
            return NotFound();

        var title = (req.Title ?? "").Trim();
        if (string.IsNullOrWhiteSpace(title))
            return BadRequest(new { message = "Title is required." });

        if (title.Length > 200)
            return BadRequest(new { message = "Title must be <= 200 characters." });

        existing.Title = title;
        existing.IsDone = req.IsDone;

        Store[id] = existing;
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        return Store.TryRemove(id, out _) ? NoContent() : NotFound();
    }
}
```

✅ Backend tamam: `GET/POST/PUT/DELETE api/todos`

---

# 2) Frontend: Todo Page (Add/Edit/Delete/Done) + AJAX (HttpClient)

## 2.1 Yeni sayfa: `ClientApp/Pages/Todo.razor`

Mevcut counter sayfanın yerine bunu koy (PageTitle dahil):

```razor
@page "/todo"
@using System.Net.Http.Json
@using System.Text.Json
@inject HttpClient Http
@inject IJSRuntime JS

<PageTitle>Todo</PageTitle>

<h3 class="mb-3">Todo</h3>

@if (!string.IsNullOrWhiteSpace(error))
{
    <div class="alert alert-danger" role="alert">@error</div>
}

<div class="card shadow-sm mb-3">
    <div class="card-body">
        <form id="todoForm" novalidate @onsubmit="AddTodo">
            <div class="row g-2 align-items-center">
                <div class="col-12 col-md-8">
                    <div class="input-group">
                        <span class="input-group-text">
                            <i class="bi bi-check2-square" aria-hidden="true"></i>
                        </span>

                        <input id="newTodoTitle"
                               class="form-control"
                               placeholder="Add a new todo..."
                               @bind="newTitle"
                               @bind:event="oninput"
                               required />

                        <div class="invalid-feedback">
                            Title cannot be empty.
                        </div>
                    </div>
                </div>

                <div class="col-12 col-md-auto">
                    <button type="submit" class="btn btn-primary w-100">
                        Add
                    </button>
                </div>
            </div>
        </form>
    </div>
</div>

@if (isLoading)
{
    <div class="d-flex align-items-center gap-2">
        <div class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></div>
        <span>Loading...</span>
    </div>
}
else if (todos.Count == 0)
{
    <div class="alert alert-secondary" role="alert">No todos yet.</div>
}
else
{
    <div class="table-responsive">
        <table class="table table-hover align-middle">
            <thead class="table-light">
                <tr>
                    <th style="width:60px;">Done</th>
                    <th>Title</th>
                    <th class="text-end" style="width:180px;">Actions</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var t in todos)
                {
                    <tr>
                        <td>
                            <input class="form-check-input"
                                   type="checkbox"
                                   checked="@t.IsDone"
                                   @onchange="(e) => ToggleDone(t, (bool)e.Value!)" />
                        </td>

                        <td>
                            @if (editId == t.Id)
                            {
                                <div class="input-group">
                                    <input id="@GetEditInputId(t.Id)"
                                           class="form-control"
                                           @bind="editTitle"
                                           @bind:event="oninput"
                                           required />
                                    <div class="invalid-feedback">Title cannot be empty.</div>
                                </div>
                            }
                            else
                            {
                                <span class="@(t.IsDone ? "text-decoration-line-through text-muted" : "")">
                                    @t.Title
                                </span>
                            }
                        </td>

                        <td class="text-end">
                            @if (editId == t.Id)
                            {
                                <button class="btn btn-sm btn-success me-2"
                                        @onclick="() => SaveEdit(t)">
                                    Save
                                </button>
                                <button class="btn btn-sm btn-outline-secondary"
                                        @onclick="CancelEdit">
                                    Cancel
                                </button>
                            }
                            else
                            {
                                <button class="btn btn-sm btn-outline-primary me-2"
                                        @onclick="() => StartEdit(t)">
                                    Edit
                                </button>
                                <button class="btn btn-sm btn-outline-danger"
                                        @onclick="() => DeleteTodo(t)">
                                    Delete
                                </button>
                            }
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    </div>
}

@code {
    private bool isLoading = true;
    private string? error;

    private List<TodoItem> todos = new();

    private string newTitle = "";

    private int? editId = null;
    private string editTitle = "";

    protected override async Task OnInitializedAsync()
    {
        await LoadTodos();
    }

    private async Task LoadTodos()
    {
        isLoading = true;
        error = null;

        try
        {
            var data = await Http.GetFromJsonAsync<List<TodoItem>>("api/todos");
            todos = data ?? new List<TodoItem>();
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }
        finally
        {
            isLoading = false;
        }
    }

    // ✅ Add (AJAX) + JS validation (no empty / whitespace)
    private async Task AddTodo()
    {
        error = null;

        // JS: prevent empty/whitespace + bootstrap invalid class
        var ok = await JS.InvokeAsync<bool>("todoForms.validateRequiredTrimmed", "newTodoTitle");
        if (!ok) return;

        var title = newTitle.Trim();

        try
        {
            var resp = await Http.PostAsJsonAsync("api/todos", new { Title = title });
            if (!resp.IsSuccessStatusCode)
            {
                error = await resp.Content.ReadAsStringAsync();
                return;
            }

            var created = await resp.Content.ReadFromJsonAsync<TodoItem>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (created is not null)
            {
                // ✅ UI update without page reload
                todos.Insert(0, created);
                newTitle = "";
                await JS.InvokeVoidAsync("todoForms.clearValidation", "newTodoTitle");
            }
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }
    }

    private void StartEdit(TodoItem t)
    {
        editId = t.Id;
        editTitle = t.Title;
    }

    private void CancelEdit()
    {
        editId = null;
        editTitle = "";
    }

    private async Task SaveEdit(TodoItem t)
    {
        error = null;

        var inputId = GetEditInputId(t.Id);
        var ok = await JS.InvokeAsync<bool>("todoForms.validateRequiredTrimmed", inputId);
        if (!ok) return;

        var title = editTitle.Trim();

        try
        {
            var resp = await Http.PutAsJsonAsync($"api/todos/{t.Id}", new { Title = title, IsDone = t.IsDone });
            if (!resp.IsSuccessStatusCode)
            {
                error = await resp.Content.ReadAsStringAsync();
                return;
            }

            var updated = await resp.Content.ReadFromJsonAsync<TodoItem>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (updated is not null)
            {
                var idx = todos.FindIndex(x => x.Id == t.Id);
                if (idx >= 0) todos[idx] = updated;

                CancelEdit();
            }
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }
    }

    private async Task ToggleDone(TodoItem t, bool isDone)
    {
        error = null;

        try
        {
            // Title değişmeden sadece done güncelliyoruz
            var resp = await Http.PutAsJsonAsync($"api/todos/{t.Id}", new { Title = t.Title, IsDone = isDone });
            if (!resp.IsSuccessStatusCode)
            {
                error = await resp.Content.ReadAsStringAsync();
                return;
            }

            var updated = await resp.Content.ReadFromJsonAsync<TodoItem>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (updated is not null)
            {
                var idx = todos.FindIndex(x => x.Id == t.Id);
                if (idx >= 0) todos[idx] = updated;
            }
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }
    }

    private async Task DeleteTodo(TodoItem t)
    {
        error = null;

        try
        {
            var resp = await Http.DeleteAsync($"api/todos/{t.Id}");
            if (!resp.IsSuccessStatusCode && resp.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                error = await resp.Content.ReadAsStringAsync();
                return;
            }

            todos.RemoveAll(x => x.Id == t.Id);
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }
    }

    private static string GetEditInputId(int id) => $"editTodoTitle_{id}";

    public sealed class TodoItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public bool IsDone { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
```

> Bu yaklaşımda  **sayfa yenilenmez** . `HttpClient` çağrısı zaten browser tarafında **fetch/XHR (AJAX)** ile gider ve listeyi lokal güncelleyerek UI’a yansıtır.

---

# 3) JS: Form/Inputs boş olmasın (trim kontrol + Bootstrap invalid)

## 3.1 JS dosyası oluştur

`ClientApp/wwwroot/js/todoForms.js` oluştur:

```js
window.todoForms = {
  validateRequiredTrimmed: function (inputId) {
    const el = document.getElementById(inputId);
    if (!el) return false;

    const value = (el.value || "").trim();
    const ok = value.length > 0;

    // Bootstrap validation styling
    el.classList.toggle("is-invalid", !ok);
    el.classList.toggle("is-valid", ok);

    return ok;
  },

  clearValidation: function (inputId) {
    const el = document.getElementById(inputId);
    if (!el) return;

    el.classList.remove("is-invalid");
    el.classList.remove("is-valid");
    el.value = "";
  }
};
```

## 3.2 Script’i projeye ekle

`ClientApp/wwwroot/index.html` içine (body kapanmadan önce) ekle:

```html
<script src="js/todoForms.js"></script>
```

---

# 4) Menüye Todo link’i ekle

`ClientApp/Shared/NavMenu.razor` içine:

```razor
<div class="nav-item px-3">
    <NavLink class="nav-link" href="todo">
        <span class="bi bi-check2-square-nav-menu" aria-hidden="true"></span> Todo
    </NavLink>
</div>
```

> Eğer `bi-check2-square-nav-menu` ikonun görünmezse, ya Bootstrap Icons CDN var ama template `*-nav-menu` custom svg sistemini kullanıyordur. O durumda `span` class’ını direkt `bi bi-check2-square` yapıp CSS gerektirmeden gösteririz.

---

# 5) Test

1. Server çalıştır:

```bash
cd ServerApp
dotnet run
```

2. Client çalıştır:

```bash
cd ClientApp
dotnet run
```

3. Tarayıcı:

* `/todo`
* Boş eklemeye çalış → **engellenmeli**
* Todo ekle → **sayfa yenilenmeden listede görünmeli**
* Edit/Save → güncellenmeli
* Delete → kaldırılmalı
* Done checkbox → üstü çizilmeli + backend’de update olmalı

---

## İstersen sonraki iyileştirme (1 mesajda eklerim)

* ✅ “Edit” modunda Enter ile Save
* ✅ “Done” olanları alta taşıma (sort)
* ✅ LocalStorage ile client cache (sayfa kapansa bile kalsın)
* ✅ Backend’i DB’ye (SQLite) bağlama

İstersen önce şunu söyle: Projeyi **hosted (tek server)** mı çalıştırıyorsun yoksa **client/server ayrı port** mu? Ona göre CORS/URL ayarını da netleştiririm.
