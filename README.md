# Fotomappa

Applicazione desktop per Windows (WPF, .NET 9) che mostra su una mappa le foto di una cartella, posizionandole in base ai dati GPS contenuti nei metadati EXIF.

## Funzionalità

- **Selezione della cartella**, con opzione per includere le sottocartelle.
- **Lettura dei dati GPS EXIF** di ogni foto (con [MetadataExtractor](https://github.com/drewnoakes/metadata-extractor-dotnet)): un marcatore per ogni foto sulla mappa.
- **Pannello foto**: selezionando un marcatore compaiono anteprima, posizione, fotocamera ed esposizione, il percorso del file e il pulsante "Mostra nella cartella".
- **Apri con il visualizzatore** predefinito di Windows.
- **Caricamento progressivo**: la mappa si aggiorna ogni 300 foto ed è possibile fermare la scansione con il pulsante "Interrompi".
- **Foto senza GPS**: un cerchio dedicato raccoglie le foto non classificate e ne apre la griglia.
- **Raggruppamento dei marcatori** (cluster) con le aree di raggruppamento tratteggiate.
- **Impostazioni dei marcatori**: dimensione e colori, anche per il cerchio delle foto senza GPS.
- **Temi grafici** selezionabili dal pulsante "Tema": Standard, Academia (Scholarly Mobile) e Retro-Futurism. Il tema scelto viene applicato sia all'interfaccia sia alla mappa e viene ricordato alla riapertura.
- Finestra informazioni con il pulsante "?".

## Requisiti

- Windows 10 o 11
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) per compilare (o il runtime .NET 9 Desktop per eseguire)
- [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/) (già incluso in Windows 11)

## Compilazione ed esecuzione

```bash
dotnet build -c Release
dotnet run -c Release
```

L'eseguibile si chiama `Fotomappa.exe` e si trova in `bin\Release\net9.0-windows\`.

## Come funziona

- La mappa è una pagina HTML con [Leaflet](https://leafletjs.com/) 1.9.4 e Leaflet.markercluster 1.5.3, inclusi in `Assets/map` e mostrati in un controllo **WebView2**. Non serve connessione per i file della libreria.
- Il codice C# comunica con la mappa eseguendo script JavaScript; la mappa risponde con messaggi (`postMessage`), ad esempio per la selezione di una foto.
- I temi sono definiti in `Services/Themes.cs` e applicati a runtime tramite risorse dinamiche WPF; la mappa riceve gli stessi colori e font come variabili CSS.

## Struttura del progetto

| Percorso | Contenuto |
|---|---|
| `MainWindow.xaml(.cs)` | Finestra principale, menu del tema, pannello foto |
| `SettingsWindow.xaml(.cs)` | Impostazioni dei marcatori |
| `AboutWindow.xaml(.cs)` | Finestra informazioni |
| `Services/PhotoScanner.cs` | Scansione delle cartelle e lettura EXIF |
| `Services/Themes.cs` | Definizione e applicazione dei temi |
| `Services/Settings.cs` | Impostazioni salvate dell'utente |
| `Services/ImageUtil.cs` | Utilità per le immagini |
| `Models/PhotoItem.cs` | Modello di una foto |
| `Assets/map/` | Pagina della mappa e librerie Leaflet |

## Dati salvati

- Impostazioni: `%APPDATA%\Fotomappa\settings.json`
- Log degli errori: `%LOCALAPPDATA%\Fotomappa\errori.log`
- Dati di WebView2: `%LOCALAPPDATA%\Fotomappa\WebView2`

## Autore

Furyo Biagioni
