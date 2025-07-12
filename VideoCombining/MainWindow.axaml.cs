using System;
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
        var path = FolderPathBox.Text;

        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            StatusTextBlock.Text = "Invalid folder selected.";
            return;
        }

        // Setup UI for processing state
        ProcessButton.IsEnabled = false;
        ChooseFolderButton.IsEnabled = false;
        ProgressBar.IsVisible = true;
        ProgressBar.Value = 0;
        StatusTextBlock.Text = "Starting...";

        try
        {
            var progress = new Progress<ProgressReport>(report =>
            {
                ProgressBar.Value = report.PercentComplete;
                StatusTextBlock.Text = report.Status;
            });

            await Task.Run(() => VideoProcessor.ProcessVideos(path, progress));
        }
        catch (Exception ex)
        {
            // Handle any unexpected errors from the processing task
            StatusTextBlock.Text = $"An error occurred: {ex.Message}";
        }
        finally
        { 
            // Restore UI to idle state
            ProcessButton.IsEnabled = true;
            ChooseFolderButton.IsEnabled = true;
            ProgressBar.IsVisible = false;
        }
    }
}