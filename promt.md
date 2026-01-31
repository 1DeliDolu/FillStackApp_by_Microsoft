
Anladım. Şu an senin akışın “kategori seçmeden todo görünmüyor”. İstediğin ise:

* **Her zaman ekranda tüm todolardan bir “önizleme” olsun (ilk 5)** ✅
* **Kategori seçince** , o kategoriye ait **tam liste** görünsün ✅

Bunu en temiz şekilde şöyle yapacağız:

1. Backend’den  **tüm todos** ’u da çekeceğiz: `GET api/todos`
2. Sağ panelde **“Recent Todos (Top 5)”** kartı **her zaman** görünecek
3. Kategori seçince altta **o kategoriye ait full liste + add/edit/delete/done** açılacak

Aşağıdaki değişiklikleri `Todo.razor` dosyana uygula.

---

# 1) @code bölümüne global todo listesi ekle

`@code { ... }` içine şu alanları ekle:

```csharp
private List<TodoItem> allTodos = new();   // tüm todolar
private IEnumerable<TodoItem> RecentTodos => allTodos.Take(5); // ilk 5 (API zaten desc dönüyor)
```

---

# 2) OnInitializedAsync: kategorilerden sonra tüm todoları yükle

Mevcut `OnInitializedAsync()` şu ise:

```csharp
protected override async Task OnInitializedAsync()
{
    await LoadCategories();
}
```

Bunu şöyle yap:

```csharp
protected override async Task OnInitializedAsync()
{
    await LoadCategories();
    await LoadAllTodos();
}
```

Ve `LoadAllTodos()` metodunu ekle:

```csharp
private async Task LoadAllTodos()
{
    error = null;
    try
    {
        allTodos = await (Api.GetFromJsonAsync<List<TodoItem>>("api/todos")
                   ?? Task.FromResult(new List<TodoItem>()));
    }
    catch (Exception ex)
    {
        error = ex.Message;
    }
}
```

---

# 3) Category badge count’larını artık allTodos’tan hesapla

Senin category listesinde şu vardı:

```razor
<span class="badge text-bg-dark">@TodoCount(c.Id)</span>
```

`TodoCount` fonksiyonunu şu hale getir (cache’yi tamamen kaldırıyoruz, daha az bug):

```csharp
private int TodoCount(int categoryId) => allTodos.Count(t => t.CategoryId == categoryId);
```

---

# 4) UI: Sağ tarafta “Recent Todos (Top 5)” kartını HER ZAMAN göster

Todo.razor’da sağ kolonun (`col-12 col-lg-8`) içine, mevcut category/todo kartının **üstüne** şu kartı ekle:

```razor
<div class="card shadow-sm mb-3">
    <div class="card-header bg-dark text-white d-flex justify-content-between align-items-center">
        <span class="fw-semibold">Recent Todos (Top 5)</span>
        <span class="badge text-bg-light">@Math.Min(5, allTodos.Count) / @allTodos.Count</span>
    </div>

    <div class="card-body p-0">
        @{
            var recent = RecentTodos.ToList();
        }

        @if (recent.Count == 0)
        {
            <div class="p-3">
                <div class="alert alert-secondary mb-0" role="alert">No todos yet.</div>
            </div>
        }
        else
        {
            <div class="table-responsive">
                <table class="table table-hover align-middle mb-0">
                    <thead class="table-light">
                        <tr>
                            <th style="width:80px;">Done</th>
                            <th>Title</th>
                            <th style="width:180px;">Category</th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach (var t in recent)
                        {
                            <tr>
                                <td>
                                    @if (t.IsDone)
                                    {
                                        <span class="badge text-bg-success">Done</span>
                                    }
                                    else
                                    {
                                        <span class="badge text-bg-secondary">Open</span>
                                    }
                                </td>
                                <td>
                                    <span class="@(t.IsDone ? "text-decoration-line-through text-muted" : "")">@t.Title</span>
                                </td>
                                <td>
                                    <span class="badge text-bg-dark">
                                        @(categories.FirstOrDefault(c => c.Id == t.CategoryId)?.Name ?? $"#{t.CategoryId}")
                                    </span>
                                </td>
                            </tr>
                        }
                    </tbody>
                </table>
            </div>
        }
    </div>
</div>
```

✅ Bu kart her zaman görünür (kategori seçmese de “ilk 5” hep içerikte olur).

---

# 5) Kategori seçince full liste açılmaya devam etsin (mevcut yapı)

Senin mevcut “Category seçili değilse uyarı, seçince add form + full list” kısmın aynen kalsın.

Sadece şu fark: artık kullanıcı kategori seçmese bile **Recent Top 5** görüyor olacak.

---

# 6) Add/Edit/Delete/Done işlemlerinde allTodos’u da güncelle

Şu an sadece `todos` listesini güncelliyorsun. Bunlara **allTodos** da eklenecek ki recent/top5 ve badge count doğru kalsın.

## 6.1 AddTodo içinde

`created` geldikten sonra sen şunu yapıyorsun:

```csharp
todos.Insert(0, created);
```

Hemen altına ekle:

```csharp
allTodos.Insert(0, created);
```

## 6.2 SaveEdit içinde

`updated` geldikten sonra `todos` update ediyorsun. Ek olarak:

```csharp
var aidx = allTodos.FindIndex(x => x.Id == t.Id);
if (aidx >= 0) allTodos[aidx] = updated;
```

## 6.3 ToggleDone içinde

Aynı şekilde:

```csharp
var aidx = allTodos.FindIndex(x => x.Id == t.Id);
if (aidx >= 0) allTodos[aidx] = updated;
```

## 6.4 DeleteTodo içinde

Sen `todos.RemoveAll(...)` yapıyorsun. Ek olarak:

```csharp
allTodos.RemoveAll(x => x.Id == t.Id);
```

✅ Böylece:

* Recent top5 anında güncellenir
* Category badge sayıları doğru kalır
* Sayfa reload olmadan her şey render edilir

---

## Sonuç

* Sayfa açılır açılmaz **Recent Todos (Top 5)** görünür ✅
* Kategori seçince o kategoriye ait **tam liste** görünür ✅
* Add/Edit/Delete/Done yaptıkça hem seçili liste hem recent/top5 **anında** güncellenir ✅
