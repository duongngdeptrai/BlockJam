using Unity.VisualScripting;
using UnityEngine;

public abstract class Door : MonoBehaviour
{
    [SerializeField] protected int size ;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    protected ColorType colorType;
    protected Direction direction;

    public ColorType ColorType { get => colorType; set => colorType = value; }
    public int Size { get => size; set => size = value; }
    public Direction Direction { get => direction; set => direction = value; }

    public virtual void SetWall()
    {
        spriteRenderer.color = ColorManager.GetColor(colorType);
        transform.rotation = Quaternion.Euler(0, 0, RotationManager.GetRotation(direction));
    }
    public virtual void UpdateColorImg(ColorType _colorType)
    {
        colorType = _colorType;
        spriteRenderer.color = ColorManager.GetColor(_colorType);
    } 
}
