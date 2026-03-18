using System;
using UnityEngine;

public abstract class Block : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer[] Img;

    private bool isClicked = false;

    public void SetColorBlock(ColorType colorType)
    {
        foreach (var spriteRenderer in Img)
        {
            spriteRenderer.color = ColorManager.GetColor(colorType);
        }
    }
}
