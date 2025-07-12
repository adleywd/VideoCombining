using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace VideoCombining;

public static class VideoProcessor
{
    /// <summary>
    /// Processes videos in the specified folder by grouping them according to their aspect ratios 
    /// and combining each group into a single video file.
    /// </summary>
    /// <param name="folderPath">The path to the folder containing the videos to process.</param>
    /// <param name="progress">An object to report progress back to the caller.</param>
    /// <remarks>
    /// This method scans the given folder for MP4 files, extracts their resolutions, and groups 
    /// them by their aspect ratios. It then combines videos with the same aspect ratio into a
    /// single video file.
    /// </remarks>
    public static void ProcessVideos(string folderPath, IProgress<ProgressReport> progress)
    {
        progress.Report(new ProgressReport (PercentComplete: 0, Status: "Gathering video files..." ));
        var files = Directory.GetFiles(folderPath, "*.mp4");

        if (files.Length == 0)
        {
            progress.Report(new ProgressReport( PercentComplete: 100, Status: "No MP4 videos found in the selected folder." ));
            return;
        }

        var validVideos = new List<VideoInfo>();
        int filesProcessed = 0;

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
            finally
            {
                filesProcessed++;
                int percentage = (int)((double)filesProcessed / files.Length * 50); // Analysis is 50% of the work
                progress.Report(new ProgressReport( PercentComplete: percentage, Status: $"Analyzing video {filesProcessed} of {files.Length}..." ));
            }
        }

        var groups = validVideos.GroupBy(v => v.AspectRatio).ToList();

        string combinedFolder = Path.Combine(folderPath, "Combined");
        Directory.CreateDirectory(combinedFolder);

        int groupsProcessed = 0;
        foreach (var group in groups)
        {
            string aspect = group.Key;
            string sanitizedAspect = aspect.Replace(':', '-');
            string tempListPath = Path.Combine(combinedFolder, $"concat_{sanitizedAspect}.txt");

            File.WriteAllLines(tempListPath, group.Select(v => $"file '{v.FilePath.Replace("'", "'\\''")}'"));

            string outputPath = Path.Combine(combinedFolder, $"combined_{sanitizedAspect}.mp4");

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
                progress.Report(new ProgressReport( PercentComplete: 100, Status: $"Failed to combine group {aspect}." ));
            }
            finally
            {
                groupsProcessed++;
                int percentage = 50 + (int)((double)groupsProcessed / groups.Count * 50); // Combining is the other 50%
                progress.Report(new ProgressReport( PercentComplete: percentage, Status: $"Combining group {groupsProcessed} of {groups.Count} ({aspect})..." ));
                File.Delete(tempListPath);
            }
        }

        progress.Report(new ProgressReport( PercentComplete: 100, Status: "Processing complete!" ));
    }


    /// <summary>
    /// Extracts the video resolution from an FFmpeg analysis of the given file.
    /// </summary>
    /// <param name="file">The file to extract the resolution from</param>
    /// <returns>The resolution of the video as a (width, height) tuple, or (0, 0) if resolution extraction fails.</returns>
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

    /// <summary>
    /// Runs ffmpeg with the given arguments and returns the output from stderr, if <paramref name="captureError"/> is true.
    /// </summary>
    /// <param name="args">The arguments to pass to ffmpeg.</param>
    /// <param name="captureError">If true, the output from stderr is captured and returned. If false, an empty string is returned.</param>
    /// <returns>The output from stderr if <paramref name="captureError"/> is true, or an empty string otherwise.</returns>
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