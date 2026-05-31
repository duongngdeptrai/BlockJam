using UnityEngine;

public static class ColorManager
{
    private static readonly Color Red = new Color(1f, 0.2f, 0.2f);
    private static readonly Color Green = new Color(0.2f, 1f, 0.2f);
    private static readonly Color Blue = new Color(0.2f, 0.2f, 1f);
    private static readonly Color Yellow = new Color(1f, 1f, 0.2f);
    private static readonly Color Purple = new Color(1f, 0.2f, 1f);
    private static readonly Color Orange = new Color(1f, 0.6f, 0.2f);

    public static Color GetColor(ColorType type, bool isEditorPreview = false)
    {
        if (isEditorPreview)
        {
            return type switch
            {
                ColorType.Red => Red,
                ColorType.Green => Green,
                ColorType.Blue => Blue,
                ColorType.Yellow => Yellow,
                ColorType.Purple => Purple,
                ColorType.Orange => Orange,
                _ => Color.white,
            };
        }

        return type switch
        {
            ColorType.Red => Color.red,
            ColorType.Green => Color.green,
            ColorType.Blue => Color.blue,
            ColorType.Yellow => Color.yellow,
            ColorType.Purple => new Color(0.5f, 0f, 0.5f),
            ColorType.Orange => Color.orange,
            _ => Color.white,
        };
    }
}

public static class RotationManager
{
    public static float GetRotation(Direction dir)
    {
        return dir switch
        {
            Direction.Up => 0,
            Direction.Right => 270,
            Direction.Down => 180,
            Direction.Left => 90,
            _ => 0,
        };
    }
}
