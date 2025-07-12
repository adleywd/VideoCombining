using System.IO;

namespace VideoCombining;

public record VideoInfo(string FilePath, int Width, int Height)
{
    public string FileName => Path.GetFileName(FilePath);
    public string AspectRatio
    {
        get
        {
            int gcd = GCD(Width, Height);
            return $"{Width / gcd}:{Height / gcd}";
        }
    }

    private static int GCD(int a, int b)
    {
        while (b != 0)
        {
            (a, b) = (b, a % b);
        }
        return a;
    }
}