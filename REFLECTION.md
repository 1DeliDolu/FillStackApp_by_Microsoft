# Reflection - InventoryHub

## Copilot Katkisi

- Entegrasyon kodu icin ilk HttpClient akislarini hizli urettim ve Copilot onerilerini baz alarak API cagrilarini ve hata yakalamayi duzenledim.
- Debug asamasinda route ve CORS kaynakli problemleri Copilot ile tespit edip duzelttim.
- JSON yapisini nested kategori ile genisletirken Copilot'un ornek yapisi hizli ilerlememi sagladi.
- Performans optimizasyonu icin Copilot, istemci ve sunucu tarafinda kisa sureli cache fikrini onerdi; uygulayip gereksiz tekrar cagriyi azalttim.

## Karsilasilan Zorluklar

- Route degisikligi ve JSON formatinin bozulmasi UI tarafinda hata uretmisti; Copilot ile hedefli try-catch ve hata mesajlari ekleyerek sorunu gorunur hale getirdim.
- CORS problemi nedeniyle istekler bloklaniyordu; Copilot yardimiyla minimal, gelistirme odakli bir CORS politikasi ekledim.

## Ogrenilenler

- Copilot, hizli prototip cikarmada ve tekrar eden boilerplate kodlarda verimi artiriyor.
- Ancak her oneri dogrudan uymayabiliyor; dogru endpoint, JSON yapisi ve guvenlik ayarlarini manuel dogrulamak gerekiyor.
- Tam yigin (front-end + back-end) gelistirirken Copilot'u kucuk, dogrulanabilir adimlarla kullanmak daha guvenli ve hizli ilerlemeyi sagliyor.
