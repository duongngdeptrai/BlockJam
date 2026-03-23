using UnityEngine;
public static class ColorManager
{
    public static Color GetColor(ColorType type)
    {
        return type switch
        {
            ColorType.Red => Color.red,
            ColorType.Green => Color.green,
            ColorType.Blue => Color.blue,
            ColorType.Yellow => Color.yellow,
            ColorType.Purple => new Color(0.5f, 0, 0.5f),
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