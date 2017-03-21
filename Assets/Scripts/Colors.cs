using System.Collections.Generic;
using UnityEngine;

public static class Colors
{
    private static List<Color> colors = new List<Color>()
    {
        Color.blue,
        Color.green,
        Color.yellow,
        Color.red,
        Color.black,
        Color.cyan,
        Color.gray
    };

    public static Color GetRandomGemColor()
    {
        int colorNum = Random.Range(0, colors.Count);
        return colors[colorNum];
    }
}
