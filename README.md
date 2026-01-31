# InventoryHub - FullStackApp

Bu klasor, Promt1.md adimlarina gore olusturulan Blazor (ClientApp) + Minimal API (ServerApp) cozumunu icerir.

## Calistirma Adimlari

1) API base URL ve cache ayari
   - Dosya: `ClientApp/wwwroot/appsettings.json`
   - Varsayilan: `http://localhost:5249`
   - Cache: `CacheSettings:ProductListSeconds` (istemci cache suresi)
   - ServerApp'in portu farkliysa bu degeri guncelleyin.

2) ServerApp calistir

```bash
cd ServerApp
dotnet run
```

3) ClientApp calistir

```bash
cd ClientApp
dotnet run
```

4) Tarayici
   - Client: `http://localhost:<client-port>/fetchproducts`
   - API: `http://localhost:5249/api/productlist`

## Kisa Kullanim Notu

- ClientApp, `FetchProducts` sayfasinda `/api/productlist` endpoint'inden veri ceker (kategori bilgisi nested gelir).
- ServerApp tarafinda CORS acik (AllowAnyOrigin/AllowAnyHeader/AllowAnyMethod).
- Guvenlik ihtiyaciniz varsa CORS politikasini sinirlandirin.
- Performance: ClientApp tarafinda kisa sureli bellek ici cache ve ServerApp tarafinda MemoryCache kullanilir.
- Cache suresi konfigurasyon ile belirlenir: `CacheSettings:ProductListSeconds`.

## Basit Performans Testi

1) Tarayici DevTools > Network tabini acin.
2) `/fetchproducts` sayfasini acin ve `/api/productlist` istegini gorun.
3) 30 saniye icinde baska bir sayfaya gidip geri donun; yeni bir istek gelmemelidir (client cache).
4) 30 saniye gectikten sonra sayfayi tekrar acin; yeni istek gorunmelidir.

## Ornek API Cevabi (HTTP)

Ornek istek:

```bash
curl http://localhost:5249/api/productlist
```

Ornek JSON:

```json
[
  {
    "id": 1,
    "name": "Laptop",
    "price": 1200.5,
    "stock": 25,
    "category": {
      "id": 101,
      "name": "Electronics"
    }
  },
  {
    "id": 2,
    "name": "Headphones",
    "price": 50.0,
    "stock": 100,
    "category": {
      "id": 102,
      "name": "Accessories"
    }
  }
]
```

## Dosyalar

- `ServerApp/Program.cs`: Minimal API ve `/api/productlist` endpoint'i
- `ClientApp/Pages/FetchProducts.razor`: UI + API cagri ve hata yonetimi
- `ClientApp/wwwroot/appsettings.json`: ApiBaseUrl ayari
