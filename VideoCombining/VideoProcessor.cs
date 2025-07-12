using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace VideoCombining;

public static class VideoProcessor
{
    public static void ProcessVideos(string folderPath)
    {
        var files = Directory.GetFiles(folderPath, "*.mp4");

        if (files.Length == 0)
        {
            Console.WriteLine("No videos found.");
            return;
        }

        var validVideos = new List<VideoInfo>();

        foreach (var file in files)
        {
            try
            {
                var (w, h) = GetVideoResolution(file);
                if (w > 0 && h > 0)
                {
                    validVideos.Add(new VideoInfo(file, w, h));
                }
                else
                {
                    Console.WriteLine($"Skipped (no resolution): {file}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to analyze {file}: {ex.Message}");
            }
        }

        var groups = validVideos.GroupBy(v => v.AspectRatio);

        string combinedFolder = Path.Combine(folderPath, "Combined");
        Directory.CreateDirectory(combinedFolder);

        foreach (var group in groups)
        {
            string aspect = group.Key;
            string tempListPath = Path.Combine(folderPath, $"concat_{aspect}.txt");

            File.WriteAllLines(tempListPath, group.Select(v => $"file '{v.FilePath.Replace("'", "'\\''")}'"));

            string outputPath = Path.Combine(combinedFolder, $"combined_{aspect.Replace(':', '-')}.mp4");

            try
            {
                // RE-ENCODE to ensure compatibility (slow but safe)
                string ffmpegArgs = $"-f concat -safe 0 -i \"{tempListPath}\" -c:v libx264 -preset fast -crf 23 -c:a aac -b:a 128k -y \"{outputPath}\"";
                RunFFmpeg(ffmpegArgs);

                Console.WriteLine($"Combined: {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to combine group {aspect}: {ex.Message}");
            }

            File.Delete(tempListPath.Replace(".txt", ""));
        }
    }


    private static (int width, int height) GetVideoResolution(string file)
    {
        string output = RunFFmpeg($"-i \"{file}\"", captureError: true);

        var match = Regex.Match(output, @"Stream #\d+:\d+.*Video:.*?(\d{2,5})x(\d{2,5})", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            int width = int.Parse(match.Groups[1].Value);
            int height = int.Parse(match.Groups[2].Value);
            return (width, height);
        }

        Console.WriteLine($"Could not extract resolution for: {file}");
        return (0, 0);
    }

    private static string RunFFmpeg(string args, bool captureError = false)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardError = captureError,
            RedirectStandardOutput = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)
                            ?? throw new InvalidOperationException("Failed to start ffmpeg");

        string output = captureError ? process.StandardError.ReadToEnd() : "";
        process.WaitForExit();

        return output;
    }
}