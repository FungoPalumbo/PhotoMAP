using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using PhotoMap.Models;
using PhotoMap.Services;

namespace PhotoMap;

public partial class MainWindow : Window
{
    readonly Settings _settings = Settings.Load();
    readonly TaskCompletionSource _mapReady = new();
    ObservableCollection<PhotoItem> _photos = new();
    int _onMap;
    bool _stopped;
    Dictionary<int, PhotoItem> _byId = new();
    CancellationTokenSource? _cts;
    string? _folder;
    bool _fromMap;
    int _bigToken;

    public MainWindow()
    {
        InitializeComponent();
        SubCheck.IsChecked = _settings.IncludeSubfolders;
        ApplyThemeText();
        BuildThemeMenu();
        Loaded += async (_, _) => await InitAsync();
    }

    async Task InitAsync()
    {
        try
        {
            SetWebBackground();
            var data = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Fotomappa", "WebView2");
            var env = await CoreWebView2Environment.CreateAsync(null, data);
            await Web.EnsureCoreWebView2Async(env);

            var core = Web.CoreWebView2;
            core.SetVirtualHostNameToFolderMapping("fotomappa.local", Path.Combine(AppContext.BaseDirectory, "map"), CoreWebView2HostResourceAccessKind.Allow);
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.IsStatusBarEnabled = false;
            core.Settings.AreDevToolsEnabled = false;
            core.WebMessageReceived += (_, e) => OnMapMessage(e.TryGetWebMessageAsString());
            core.NavigationCompleted += (_, _) => _mapReady.TrySetResult();
            core.Navigate("https://fotomappa.local/map.html");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Impossibile avviare la mappa (serve il runtime WebView2 di Microsoft Edge).\n\n" + ex.Message,
                "Fotomappa", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        ApplyMapStyle();
        await Js("setMessage('Scegli una cartella per vedere le foto sulla mappa')");
        if (!string.IsNullOrEmpty(_settings.LastFolder) && Directory.Exists(_settings.LastFolder))
            await LoadFolderAsync(_settings.LastFolder);
    }

    async Task Js(string script)
    {
        await _mapReady.Task;
        await Web.CoreWebView2.ExecuteScriptAsync(script);
    }

    void PickFolder_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFolderDialog { Title = "Scegli la cartella con le foto", Multiselect = false };
        if (_folder != null) dlg.InitialDirectory = _folder;
        if (dlg.ShowDialog(this) == true) _ = LoadFolderAsync(dlg.FolderName);
    }

    void SubCheck_Click(object sender, RoutedEventArgs e)
    {
        _settings.IncludeSubfolders = SubCheck.IsChecked == true;
        _settings.Save();
        if (_folder != null) _ = LoadFolderAsync(_folder);
    }

    async Task LoadFolderAsync(string folder)
    {
        _cts?.Cancel();
        var cts = _cts = new CancellationTokenSource();
        _stopped = false;

        _folder = folder;
        _settings.LastFolder = folder;
        _settings.Save();
        FolderText.Text = folder;
        FolderText.ToolTip = folder;

        HideNoGpsGrid();
        _photos = new ObservableCollection<PhotoItem>();
        _byId = new Dictionary<int, PhotoItem>();
        _onMap = 0;
        PhotoList.ItemsSource = _photos;
        ShowPhoto(null);
        FitButton.IsEnabled = false;
        StatsText.Text = "Lettura in corso…";
        MapSummary.Text = "Lettura delle foto in corso…";
        ProgressPanel.Visibility = Visibility.Visible;
        StopButton.IsEnabled = true;
        Progress.Value = 0;
        ProgressText.Text = "Cerco le foto…";
        await Js("clearPhotos(); setMessage(''); setNoGps(0)");
        if (cts != _cts) return;

        var progress = new Progress<(int Done, int Total)>(p =>
        {
            if (cts != _cts) return;
            Progress.Maximum = Math.Max(1, p.Total);
            Progress.Value = p.Done;
            if (!_stopped) ProgressText.Text = p.Total == 0 ? "Nessuna foto trovata" : $"{p.Done} di {p.Total} foto lette";
        });

        try
        {
            await PhotoScanner.ScanAsync(folder, SubCheck.IsChecked == true, progress,
                batch => Dispatcher.InvokeAsync(() => ApplyBatchAsync(cts, batch)).Task.Unwrap(), cts.Token);
        }
        catch (Exception ex)
        {
            if (cts != _cts) return;
            ProgressPanel.Visibility = Visibility.Collapsed;
            StatsText.Text = "Impossibile leggere la cartella";
            MapSummary.Text = ex.Message;
            return;
        }
        if (cts != _cts) return;

        ProgressPanel.Visibility = Visibility.Collapsed;
        UpdateSummary(false);
        if (_photos.Count == 0)
        {
            MapSummary.Text = "Nessuna immagine trovata nella cartella scelta";
            await Js("setMessage('Nessuna immagine trovata in questa cartella')");
        }
        else if (_onMap == 0)
            await Js("setMessage('Nessuna foto di questa cartella contiene coordinate GPS')");
        else if (PhotoList.SelectedItem == null)
            await Js("fitAll()");
    }

    async Task ApplyBatchAsync(CancellationTokenSource cts, List<PhotoItem> batch)
    {
        if (cts != _cts) return;
        foreach (var p in batch) { _photos.Add(p); _byId[p.Id] = p; }

        var located = batch.Where(p => p.HasLocation).ToList();
        bool firstOnMap = _onMap == 0 && located.Count > 0;
        _onMap += located.Count;
        UpdateSummary(true);
        BuildGridRows(true);
        if (located.Count == 0) return;

        var json = JsonSerializer.Serialize(located.Select(p => new { id = p.Id, lat = p.Latitude, lng = p.Longitude, t = p.ThumbUri }));
        await Js($"addPhotos({json})");
        if (firstOnMap) await Js("fitAll()");
        FitButton.IsEnabled = true;
    }

    void UpdateSummary(bool loading)
    {
        string more = loading ? "…" : _stopped ? " · lettura interrotta" : "";
        StatsText.Text = $"{_photos.Count} foto · {_photos.Count - _onMap} senza posizione{more}";
        MapSummary.Text = $"{_onMap} foto sulla mappa{more}";
        _ = Js($"setNoGps({_photos.Count - _onMap})");
    }

    void Stop_Click(object sender, RoutedEventArgs e)
    {
        _stopped = true;
        StopButton.IsEnabled = false;
        ProgressText.Text = "Interruzione in corso…";
        _cts?.Cancel();
    }

    void OnMapMessage(string? msg)
    {
        if (msg == "nogps") { ShowNoGpsGrid(); return; }
        if (msg == null || !msg.StartsWith("select:")) return;
        if (!int.TryParse(msg.AsSpan(7), out var id) || !_byId.TryGetValue(id, out var photo)) return;
        _fromMap = true;
        PhotoList.SelectedItem = photo;
        PhotoList.ScrollIntoView(photo);
        _fromMap = false;
    }

    // ---- grid of the photos without GPS data, shown in place of the map
    const double CellWidth = 136;
    int _gridCols;

    void ShowNoGpsGrid()
    {
        Web.Visibility = Visibility.Collapsed;
        NoGpsView.Visibility = Visibility.Visible;
        BuildGridRows(true);
    }

    void HideNoGpsGrid()
    {
        if (NoGpsView.Visibility != Visibility.Visible) return;
        NoGpsView.Visibility = Visibility.Collapsed;
        Web.Visibility = Visibility.Visible;
    }

    void BuildGridRows(bool force)
    {
        if (NoGpsView.Visibility != Visibility.Visible) return;
        int cols = Math.Max(1, (int)((NoGpsList.ActualWidth - 40) / CellWidth));
        if (!force && cols == _gridCols) return;
        _gridCols = cols;

        var items = _photos.Where(p => !p.HasLocation).ToList();
        NoGpsCountText.Text = $"{items.Count} {(items.Count == 1 ? "foto" : "foto")} · doppio clic per aprirla";
        NoGpsList.ItemsSource = items.Chunk(cols).Select(c => new GridRow { Items = c }).ToList();
    }

    void NoGpsList_SizeChanged(object sender, SizeChangedEventArgs e) => BuildGridRows(false);

    void BackToMap_Click(object sender, RoutedEventArgs e) => HideNoGpsGrid();

    void Cell_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: PhotoItem p }) return;
        if (e.ClickCount >= 2) { OpenPhoto(p); return; }
        PhotoList.SelectedItem = p;
        PhotoList.ScrollIntoView(p);
    }

    void PhotoList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        foreach (var p in e.RemovedItems.OfType<PhotoItem>()) p.IsSelected = false;
        foreach (var p in e.AddedItems.OfType<PhotoItem>()) p.IsSelected = true;
        var photo = PhotoList.SelectedItem as PhotoItem;
        if (photo is { HasLocation: true }) HideNoGpsGrid();
        ShowPhoto(photo);
        if (photo is { HasLocation: true } && !_fromMap) _ = Js($"selectPhoto({photo.Id}, true)");
        else if (photo is { HasLocation: false }) _ = Js("selectPhoto(-1, false)");
    }

    async void ShowPhoto(PhotoItem? p)
    {
        int token = ++_bigToken;
        EmptyState.Visibility = p == null ? Visibility.Visible : Visibility.Collapsed;
        DetailState.Visibility = p == null ? Visibility.Collapsed : Visibility.Visible;
        if (p == null) { BigImage.Source = null; return; }

        NameText.Text = p.FileName;
        DateText.Text = p.DateText;
        CoordText.Text = p.CoordText;
        CameraText.Text = p.CameraText;
        ExposureText.Text = p.ExposureText;
        FolderInfoText.Text = p.Folder;
        CenterButton.Visibility = p.HasLocation ? Visibility.Visible : Visibility.Collapsed;
        BigImage.Source = p.Thumb;

        try
        {
            var full = await Task.Run(() => ImageUtil.Load(p.Path, 1800, p.Orientation));
            if (token == _bigToken) BigImage.Source = full;
        }
        catch { }
    }

    void Fit_Click(object sender, RoutedEventArgs e)
    {
        HideNoGpsGrid();
        _ = Js("fitAll()");
    }

    void Center_Click(object sender, RoutedEventArgs e)
    {
        if (PhotoList.SelectedItem is PhotoItem { HasLocation: true } p) _ = Js($"selectPhoto({p.Id}, true)");
    }

    void ApplyMapStyle()
    {
        var vars = System.Text.Json.JsonSerializer.Serialize(Themes.Current.Map);
        _ = Js($"applyTheme({vars});applyStyle({_settings.MarkerSize}, '{_settings.MarkerColorOrDefault()}', '{_settings.NoGpsColorOrDefault()}')");
    }

    // ---- theme menu
    void SetWebBackground()
    {
        try
        {
            var c = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(Themes.Current.Map["--map-bg"]);
            Web.DefaultBackgroundColor = System.Drawing.Color.FromArgb(c.R, c.G, c.B);
        }
        catch { }
    }

    void ApplyThemeText()
    {
        bool up = Themes.Current.UpperLabels;
        LblPosition.Text = up ? "POSIZIONE" : "Posizione";
        LblCamera.Text = up ? "FOTOCAMERA" : "Fotocamera";
        LblExposure.Text = up ? "ESPOSIZIONE" : "Esposizione";
        LblFolder.Text = up ? "CARTELLA" : "Cartella";
    }

    void BuildThemeMenu()
    {
        ThemeList.Children.Clear();
        foreach (var t in Themes.All)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal };
            row.Children.Add(new TextBlock { Text = t == Themes.Current ? "✓" : "", Width = 22, VerticalAlignment = VerticalAlignment.Center });
            row.Children.Add(new TextBlock { Text = t.Name, VerticalAlignment = VerticalAlignment.Center });
            var b = new Button { Style = (Style)FindResource("MenuItemButton"), Content = row, Tag = t.Id };
            System.Windows.Automation.AutomationProperties.SetName(b, "Tema " + t.Name);
            b.Click += (_, _) => { ThemePopup.IsOpen = false; SetTheme(t.Id); };
            ThemeList.Children.Add(b);
        }
    }

    void Theme_Click(object sender, RoutedEventArgs e) => ThemePopup.IsOpen = !ThemePopup.IsOpen;

    void SetTheme(string id)
    {
        Themes.Apply(id);
        _settings.Theme = Themes.Current.Id;
        _settings.Save();
        ApplyThemeText();
        BuildThemeMenu();
        SetWebBackground();
        ApplyMapStyle();
    }

    void Settings_Click(object sender, RoutedEventArgs e) => new SettingsWindow(_settings, ApplyMapStyle) { Owner = this }.ShowDialog();

    void About_Click(object sender, RoutedEventArgs e) => new AboutWindow { Owner = this }.ShowDialog();

    void Open_Click(object sender, RoutedEventArgs e)
    {
        if (PhotoList.SelectedItem is PhotoItem p) OpenPhoto(p);
    }

    void OpenPhoto(PhotoItem p)
    {
        try { Process.Start(new ProcessStartInfo(p.Path) { UseShellExecute = true }); }
        catch (Exception ex) { MessageBox.Show(this, "Impossibile aprire la foto.\n\n" + ex.Message, "Fotomappa"); }
    }

    void Reveal_Click(object sender, RoutedEventArgs e)
    {
        if (PhotoList.SelectedItem is PhotoItem p)
            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{p.Path}\"") { UseShellExecute = true });
    }
}

public sealed class GridRow
{
    public PhotoItem[] Items { get; init; } = Array.Empty<PhotoItem>();
}
