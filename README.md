
# InventoryHub

InventoryHub is a simple full-stack demo application built with:

- **Blazor WebAssembly** (front-end) — `ClientApp`
- **ASP.NET Core Minimal API** (back-end) — `ServerApp`

The API returns a standardized JSON product list including a nested `category` object, and the front-end consumes and displays it. The project also includes integration debugging (route/CORS/JSON handling) and performance optimizations (client/server caching).

---

## Features

- **End-to-end integration** between Blazor WASM and Minimal API
- **Stable API contract**: `id`, `name`, `price`, `stock`, and nested `category { id, name }`
- **Debug fixes**
  - Updated API route: `/api/products` → `/api/productlist`
  - CORS configuration to allow the front-end to call the API (when running separately)
  - JSON error handling to prevent UI breakage on invalid responses
- **Performance optimizations**
  - Reduced redundant API calls in the front-end (light caching)
  - Back-end caching to reduce repeated work (optional TTL/ETag depending on implementation)

---

## Project Structure

- `ClientApp/` — Blazor WebAssembly front-end
- `ServerApp/` — Minimal API back-end
- `FullStackSolution.sln` — Solution file
- `REFLECTION.md` — Summary of how Microsoft Copilot assisted across all activities

---

## API

### GET `/api/productlist`

Example response:

```json
[
  {
    "id": 1,
    "name": "Laptop",
    "price": 1200.5,
    "stock": 25,
    "category": { "id": 101, "name": "Electronics" }
  },
  {
    "id": 2,
    "name": "Headphones",
    "price": 50,
    "stock": 100,
    "category": { "id": 102, "name": "Accessories" }
  }
]
```
