using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace VideoCombining;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void ChooseFolder_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog();
        var path = await dialog.ShowAsync(this);
        if (!string.IsNullOrWhiteSpace(path))
        {
            FolderPathBox.Text = path;
        }
    }

    private async void Process_Click(object? sender, RoutedEventArgs e)
    {
        var path = FolderPathBox.Text; // <-- Capture na thread correta

        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            await MessageBox("Invalid folder selected.");
            return;
        }

        await Task.Run(() => VideoProcessor.ProcessVideos(path)).ConfigureAwait(true);

        await MessageBox("Video processing complete.").ConfigureAwait(false);
    }

    private async Task MessageBox(string message)
    {
        var dlg = new Window
        {
            Width = 300,
            Height = 100,
            Content = new TextBlock
            {
                Text = message,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            }
        };
        await dlg.ShowDialog(this).ConfigureAwait(false);
    }
}