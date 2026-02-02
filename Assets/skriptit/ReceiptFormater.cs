using UnityEngine;

public static class ReceiptFormatter
{
    public static string Line(string left, string right, int width = 34)
    {
        left ??= "";
        right ??= "";

        if (right.Length >= width)
            return right.Substring(0, width);

        int maxLeft = width - right.Length - 1;
        if (maxLeft < 0) maxLeft = 0;

        if (left.Length > maxLeft)
            left = left.Substring(0, maxLeft);

        return left.PadRight(width - right.Length) + right;
    }

    public static string Sep(int width = 34) => new string('-', width);
}
