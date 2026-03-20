using System;
using System.Drawing;
using UnityEngine;

public abstract class Block : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer[] Img;
    [SerializeField] protected int sizeX = 1, sizeY = 1;

    private ColorType colorType; // Lưu giữ data màu thật 

    public int SizeX { get => sizeX; }
    public int SizeY { get => sizeY; }
    public ColorType ColorType { get => colorType; }

    private bool isClicked = false;

    protected virtual void OnSwapSize()
    {
        var temp = sizeX;
        sizeX = sizeY;
        sizeY = temp;
    }


    public void SetColorBlock(ColorType colorType)
    {
        // Nhận màu từ JSON (do GenBlocks() truyền xuống) và ghi nhớ vào block!
        this.colorType = colorType; 

        foreach (var spriteRenderer in Img)
        {
            spriteRenderer.color = ColorManager.GetColor(colorType);
        }
    }
}
