using Unity.VisualScripting;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private ColorType colorType;
    [SerializeField] private int size ;
    [SerializeField] private Direction direction;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public ColorType ColorType { get => colorType; set => colorType = value; }
    public int Size { get => size; set => size = value; }
    public Direction Direction { get => direction; set => direction = value; }

    public void SetWall()
    {
        spriteRenderer.color = Color.red;
        transform.rotation = Quaternion.Euler(0, 0, GetRotation(direction));
    }
    private Color GetColor(ColorType type)
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
    private float GetRotation(Direction dir)
    {
        return dir switch
        {
            Direction.Up => 0,
            Direction.Right => -90,
            Direction.Down => 180,
            Direction.Left => 90,
            _ => 0,
        };
    }
    public void UpdateColorImg(ColorType _colorType)
    {
        colorType = _colorType;
        spriteRenderer.color = GetColor(_colorType);
    } 
}
