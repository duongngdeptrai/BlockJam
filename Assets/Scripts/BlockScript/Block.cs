using System;
using System.Drawing;
using UnityEngine;

public abstract class Block : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer[] Img;
    [SerializeField] protected int sizeX = 1, sizeY = 1;

    private bool isClicked = false;

    protected virtual void OnSwapSize()
    {
        var temp = sizeX;
        sizeX = sizeY;
        sizeY = temp;
    }


    public void SetColorBlock(ColorType colorType)
    {
        foreach (var spriteRenderer in Img)
        {
            spriteRenderer.color = ColorManager.GetColor(colorType);
        }
    }
}
