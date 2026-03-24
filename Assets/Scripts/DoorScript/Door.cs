using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Door : MonoBehaviour
{
    [SerializeField] protected int size ;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] private List<ParticleSystem> particleSystems;
    protected ColorType colorType;
    public Direction direction;

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
    public void PlayParticles()
    {
        if (particleSystems == null) return;
        foreach (var ps in particleSystems)
        {
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = ColorManager.GetColor(colorType);
                ps.Play();
            }
        }
    }
    public void PlayParticles(Bounds blockBounds){
        if (particleSystems == null) return;
        foreach (var ps in particleSystems)
        {
            switch(direction){
                case Direction.Up:
                    if(ps.transform.position.x > blockBounds.min.x && ps.transform.position.x < blockBounds.max.x)
                    {
                        var main = ps.main;
                        main.startColor = ColorManager.GetColor(colorType);
                        ps.Play();
                    }
                    break;
                case Direction.Down:
                    if(ps.transform.position.x > blockBounds.min.x && ps.transform.position.x < blockBounds.max.x)
                    {
                        var main = ps.main;
                        main.startColor = ColorManager.GetColor(colorType);
                        ps.Play();
                    }
                    break;
                case Direction.Right:
                    if(ps.transform.position.y > blockBounds.min.y && ps.transform.position.y < blockBounds.max.y)
                    {
                        var main = ps.main;
                        main.startColor = ColorManager.GetColor(colorType);
                        ps.Play();
                    }
                    break;
                case Direction.Left:
                    if(ps.transform.position.y > blockBounds.min.y && ps.transform.position.y < blockBounds.max.y)
                    {
                        var main = ps.main;
                        main.startColor = ColorManager.GetColor(colorType);
                        ps.Play();
                    }
                    break;
            }
        }
    }
}
