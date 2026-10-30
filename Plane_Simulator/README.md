# Plane — C# Windows Forms təyyarə simulyatoru

Nümunə şəklə əsasən hazırlanmış, bütün şəkil və səs faylları içində olan **tam Visual Studio layihəsi**.

## Açılışı

1. Windows-da **Visual Studio 2022** açın; **.NET desktop development** iş yükü və **.NET 8 SDK** quraşdırılmış olsun.
2. `PlaneSimulator.sln` faylını açın.
3. **F5** (Start) basın.
4. `Assets` qovluğunun yerini dəyişməyin: proqram işə düşəndə fayllar avtomatik kopyalanır.

## Düymələr

| Düymə | Funksiya | Səs faylı |
|---|---|---|
| Eniş hazırlığı | Eniş elanı | `inishazirliklari.wav` |
| Turbulans | Turbulans elanı və qısa silkələnmə | `turbulans.wav` |
| Kəmərlər | Təhlükəsizlik kəməri elanı | `kemerler.wav` |
| Təşəkkürlər | Sərnişinlərə təşəkkür | `tesekkurler.wav` |
| Beep | Qısa siqnal | `beep.wav` |
| GAS | Mühərrik aktiv ikən sürəti artırır (basılı saxlamaq olar) | — |
| Break | Sürəti azaldır (basılı saxlamaq olar) | — |
| ON/OFF | Mühərriki və yanıb-sönən işığı açır/söndürür | `isikacmakapamasesi.wav` |

Klaviatura: **W / ↑** qaz, **S / ↓** əyləc, **Space** ON/OFF.

## Struktur

- `PlaneSimulator/Program.cs` — proqramın başlanğıcı
- `PlaneSimulator/PlaneForm.cs` — düymələr, audio, sürət və idarəetmə
- `PlaneSimulator/PlaneCanvas.cs` — təyyarənin və işığın çəkilməsi
- `PlaneSimulator/Assets/` — göndərilən orijinal PNG/WAV faylları

**Qeyd:** Bu mənbə kodu və layihədir, əvvəlcədən kompilyasiya edilmiş `.exe` deyil. `.exe` yaratmaq üçün Visual Studio-da `Build > Build Solution` seçin. Platforma Windows-dur.
