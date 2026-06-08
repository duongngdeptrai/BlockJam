using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Door : MonoBehaviour
{
    [SerializeField] protected int size;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] private List<ParticleSystem> particleSystems;
    protected ColorType colorType;
    public Direction direction;

    public ColorType ColorType { get => colorType; set => colorType = value; }
    public int Size { get => size; set => size = value; }
    public Direction Direction { get => direction; set => direction = value; }

    public virtual void SetWall()
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("Door: spriteRenderer is null! Assign SpriteRenderer in Inspector.", this);
            return;
        }
        spriteRenderer.color = ColorManager.GetColor(colorType);
        transform.rotation = Quaternion.Euler(0, 0, RotationManager.GetRotation(direction));
    }

    public virtual void UpdateColorImg(ColorType _colorType)
    {
        colorType = _colorType;
        if (spriteRenderer == null)
        {
            Debug.LogError("Door: spriteRenderer is null! Assign SpriteRenderer in Inspector.", this);
            return;
        }
        spriteRenderer.color = ColorManager.GetColor(_colorType);
    }

    public void PlayParticles()
    {
        if (particleSystems == null || particleSystems.Count == 0) return;

        foreach (var ps in particleSystems)
        {
            if (ps == null)
            {
                Debug.LogWarning("Door: A ParticleSystem in the list is null. Check Inspector assignment.", this);
                continue;
            }
            var main = ps.main;
            main.startColor = ColorManager.GetColor(colorType);
            ps.Play();
        }
    }

    public void PlayParticles(Bounds blockBounds)
    {
        if (particleSystems == null || particleSystems.Count == 0) return;

        foreach (var ps in particleSystems)
        {
            if (ps == null)
            {
                Debug.LogWarning("Door: A ParticleSystem in the list is null. Check Inspector assignment.", this);
                continue;
            }

            bool shouldPlay = direction switch
            {
                Direction.Up => ps.transform.position.x > blockBounds.min.x && ps.transform.position.x < blockBounds.max.x,
                Direction.Down => ps.transform.position.x > blockBounds.min.x && ps.transform.position.x < blockBounds.max.x,
                Direction.Right => ps.transform.position.y > blockBounds.min.y && ps.transform.position.y < blockBounds.max.y,
                Direction.Left => ps.transform.position.y > blockBounds.min.y && ps.transform.position.y < blockBounds.max.y,
                _ => false
            };

            if (shouldPlay)
            {
                var main = ps.main;
                main.startColor = ColorManager.GetColor(colorType);
                ps.Play();
            }
        }
    }
}
