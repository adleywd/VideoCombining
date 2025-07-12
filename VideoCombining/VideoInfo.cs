using System.IO;

namespace VideoCombining;

public record VideoInfo(string FilePath, int Width, int Height)
{
    public string AspectRatio
    {
        get
        {
            int gcd = GCD(Width, Height);
            return $"{Width / gcd}:{Height / gcd}";
        }
    }

/// <summary>
/// Calculates the greatest common divisor (GCD) of two integers using the Euclidean algorithm.
/// </summary>
/// <param name="a">The first integer.</param>
/// <param name="b">The second integer.</param>
/// <returns>The greatest common divisor of the two integers.</returns>
    private static int GCD(int a, int b)
    {
        while (b != 0)
        {
            (a, b) = (b, a % b);
        }
        return a;
    }
}