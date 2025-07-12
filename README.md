# Video Combiner

This is an Avalonia application for Windows that allows you to combine multiple videos. You can either group videos by their aspect ratio and combine them, or merge all videos into a single file while preserving each video's original aspect ratio by adding padding.

## Pre-requisites

- FFmpeg installed and in the system path (https://ffmpeg.org/download.html)

## Usage

1.  Select or drop a folder containing your video files.
2.  Choose your desired function:
    *   **Combine by Aspect Ratio**: Leave the checkbox unchecked. The application will group videos with the same aspect ratio and combine each group into a separate file.
    *   **Merge All Videos**: Check the "Merge all videos into a single file" checkbox. This will combine all videos into one file, preserving their original aspect ratios with black borders.
3.  Click the "Process Videos" button to start.
4.  A `Combined` folder will be created in the source directory containing the output video(s).

## Features

- **Two Combining Modes**: 
  - Combine videos by aspect ratio into separate files.
  - Merge all videos in a folder into a single file.
- **Aspect Ratio Preservation**: Keeps the original aspect ratio of all videos when merging by adding padding (black bars).
- **Progress Reporting**: Displays a progress bar and status updates during processing.
- **Temp File Management**: Includes an option to automatically delete temporary files after processing.

## Limitations

- The application currently only processes `.mp4` files.
- It assumes all videos to be processed are in the same folder.

## Contributing

Contributions are welcome! If you find a bug or have a suggestion, please open an issue or submit a pull request.

## License

This project is licensed under the MIT License.