using System.Reflection;
using System.Windows;

namespace PhotoMap;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        var v = Assembly.GetExecutingAssembly().GetName().Version;
        VersionText.Text = $"Versione {v?.ToString(3) ?? "1.0.0"}";
    }

    void Close_Click(object sender, RoutedEventArgs e) => Close();
}
