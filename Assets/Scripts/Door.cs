using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

public class Door : MonoBehaviour
{
    [SerializeField] private ColorType colorType;
    [SerializeField] private int size ;
    [SerializeField] private Direction direction;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private List<ParticleSystem> particleSystems;
    public ColorType ColorType { get => colorType; set => colorType = value; }
    public int Size { get => size; set => size = value; }
    public Direction Direction { get => direction; set => direction = value; }

    public void SetWall()
    {
        spriteRenderer.color = ColorManager.GetColor(colorType);
        transform.rotation = Quaternion.Euler(0, 0, RotationManager.GetRotation(direction));
    }
    public void UpdateColorImg(ColorType _colorType)
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
}
