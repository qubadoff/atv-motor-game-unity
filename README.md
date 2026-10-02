# Old Atv Motor (BurnGame)

Proje klasoru `burngetter` adini tasir; oyunun adi **Old Atv Motor**, yapimci **BurnGame**.

Siyah-beyaz, 2D ATV surus oyunu (Unity 6.3 LTS). 50 seviye, zamana karsi yaris: bitis bayragina ulas, hedef surenin altinda kalirsan 3 yildiz.
Kaskin yere degerse devrilirsin. Ilk seviyeden itibaren tumsekler ve cukurlar var; havada gazi birakmazsan sirt ustu dusersin.

## Yildizlar
- 3 yildiz: `length / parSpeed` saniyenin altinda (`LevelConfig.ThreeStarTime`), 2 yildiz: bunun 1.45 kati, 1 yildiz: bitirmek.
- HUD'daki sayac altinda canli hedef: sure hedefi gectikce yildizlar soner.
- Kalibrasyon: kazasiz suren test botu (PlaytestRunner) 1. seviyeyi ~58 sn'de bitirir (3 yildiz siniri 64 sn). Duz gaz basan bot (`BURN_BOT=dumb`) 15. saniyede devrilir.

## Akis
0. **Splash sahnesi**: BurnGame logosu (lastik icinde alev) ve yazi, ~2.5 sn sonra menuye gecer (dokununca atlanir).
1. **Menu sahnesi**: ilk aciliste isim sorar, sonra OYNA (50 seviye, 1 acik digerleri kilitli) ve AYARLAR (dil, muzik, ses) sekmeleri.
2. **Game sahnesi**: secili seviyeyi oynatir. Bitis bayragina ulasinca sonraki seviye acilir, en iyi sure kaydedilir.
   Surus sirasinda gokyuzunde motivasyon cumleleri belirir (her turda 4), bulutlar ve kus suruleri gecer.

## Diller
Azerbaycanca, Ingilizce, Rusca, Turkce. Varsayilan **Otomatik**: cihaz dili (`Loc.DetectDeviceLanguage`, once isletim sistemi yerel ayari, sonra Unity systemLanguage).
Ayarlardan sabit bir dil secilebilir; "Otomatik" secilince tekrar cihaz diline doner.
Metinler `Assets/Scripts/Loc.cs`, motivasyon cumleleri `Assets/Scripts/Quotes.cs` icinde.

## Muzik
`Assets/Audio/Music` altindaki parcalar Kevin MacLeod'a aittir (CC BY 4.0), bkz. `CREDITS.md`. Menu sahnesindeki `MusicPlayer` sahneler arasi yasar ve karisik sirayla calar.

## Kontroller
- **Gaz:** Sag ok / D / W, dokunmatikte ekranin sag yarisi
- **Fren / geri:** Sol ok / A / S, dokunmatikte sol yarisi
- Havada gaz burnu kaldirir, fren burnu indirir.
- Oyun sonu: **Tekrar dene** (R / Space) veya **Menu** (Esc). Seviye sonu: **Sonraki level** (Space).

## Proje yapisi
- `Assets/Scenes/Menu.unity`, `Assets/Scenes/Game.unity`
- `Assets/Scripts/AtvController.cs` — motor, fren, egilme, anti-takla, giris
- `Assets/Scripts/EngineAudio.cs` — kodla sentezlenen motor sesi (ses dosyasi yok)
- `Assets/Scripts/Levels.cs` — 50 seviyenin zorluk egrisi ve `LevelSession`
- `Assets/Scripts/PlayerProfile.cs` — isim, acik seviye, en iyi sureler (PlayerPrefs)
- `Assets/Scripts/TerrainGenerator.cs` — seviyeye gore arazi (Perlin + tumsekler), bitisten sonra duzlesir
- `Assets/Scripts/GameManager.cs` — HUD, bitis bayragi, seviye tamamlama, oyun sonu
- `Assets/Scripts/MenuController.cs` — isim ekrani, OYNA/AYARLAR sekmeleri (arayuz kodla kurulur)
- `Assets/Scripts/Loc.cs`, `Quotes.cs`, `Settings.cs`, `MusicPlayer.cs` — dil, sozler, ayarlar, muzik
- `Assets/Scripts/QuoteDisplay.cs`, `SkyDecor.cs` — gokyuzu sozleri, bulut ve kuslar
- `Assets/Editor/SpriteFactory.cs` — ATV, tekerlek, bayrak, kilit, bulut, kus sprite'larini kodla cizer (onizleme: `Previews/atv_preview.png`)
- `Assets/Editor/TerrainAudit.cs` — seviyelerin azami egimini olcer (menu: **BurnGetter > Terrain Audit**)
- `Assets/Editor/SceneBuilder.cs` — iki sahneyi sifirdan kurar (menu: **BurnGetter > Build Game Scenes**)
- `Assets/Editor/PlaytestRunner.cs` — komut satirindan otomatik surus testi

## Komut satiri
```
UNITY="/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity"
# Sprite'lari ve sahneleri sifirdan kur (editor kapali olmali)
"$UNITY" -batchmode -projectPath "$PWD" -executeMethod SceneBuilder.Build -quit -logFile build.log
# Otomatik test: bitise kadar surus (akilli bot), secili seviyede; BURN_BOT=dumb ile sadece-gaz botu
BURN_LEVEL=25 "$UNITY" -batchmode -projectPath "$PWD" -executeMethod PlaytestRunner.Run -logFile playtest.log
# Menu sahnesi: her dil x (isim, oyna, ayarlar) sayfalarinda metin tasma/binme denetimi (UiAudit)
BURN_SCENE=Menu "$UNITY" -batchmode -projectPath "$PWD" -executeMethod PlaytestRunner.Run -logFile menu.log
```
Not: Script varsayilanlarini degistirince sahnedeki kayitli degerler guncellenmez; `SceneBuilder.Build` ile sahneyi yeniden kurun.

## Android APK
```
"$UNITY" -batchmode -buildTarget Android -projectPath "$PWD" -executeMethod BuildScript.BuildAndroid -quit -logFile android.log
```
Cikti: `Builds/Android/OldAtvMotor.apk` (IL2CPP, ARM64 + ARMv7, minSdk 24). Menu: **BurnGetter > Build Android APK**.

## Uygulama kimligi
- iOS: `org.burngame.atv-motor`, Android: `org.burngame.atvmotor` (Android paket adinda tire gecersiz), sirket: BurnGame.
- Uygulama ikonu `Assets/Sprites/logo.png`; Unity acilis logosu kapali (Unity 6'da tum lisanslarda serbest).

## Ayar onerileri
- Fizik: `ATV` nesnesinde `AtvController` — `motorTorque`, `maxSpeed`, `airTorque`, `antiFlipAngle`
- Zorluk egrisi: `Assets/Scripts/Levels.cs` icindeki `Levels.Get`
- Ses: `ATV` nesnesinde `EngineAudio` — `idlePitch`, `maxPitch`, `throttleVolume`
