using System;
using System.Drawing;
using TMPro;
using UnityEngine;

public abstract class Block : MonoBehaviour
{
    void Awake()
    {
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();

        foreach (var img in sprites)
        {
            img.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
        }
    }
    [SerializeField] protected SpriteRenderer[] Img;
    [SerializeField] protected int sizeX = 1, sizeY = 1;
    [SerializeField] protected Transform MineContainer;
    [SerializeField] protected TextMeshPro mineCountText;
    [SerializeField] protected Transform SecondColorContainer;
    [SerializeField] protected SpriteRenderer[] SecondColorSprites;

    public ColorType colorType; // Lưu giữ data màu thật 

    public int SizeX { get => sizeX; }
    public int SizeY { get => sizeY; }
    public ColorType ColorType { get => colorType; }

    protected virtual void OnSwapSize()
    {
        var temp = sizeX;
        sizeX = sizeY;
        sizeY = temp;
    }


    public void SetColorBlock(ColorType colorType)
    {
        this.colorType = colorType; 

        foreach (var spriteRenderer in Img)
        {
            spriteRenderer.color = ColorManager.GetColor(colorType);
        }
    }

     private bool HasMine;

    private int MineCount;

    public int GetMineCount()
    {
        return MineCount;
    }

    public bool GetHasMine()
    {
        return HasMine;
    }

    public void SetHasMine(bool hasMine)
    {
        HasMine = hasMine;
        MineContainer.gameObject.SetActive(hasMine);
    }

    public void SetMineCount(int mineCount)
    {
        MineCount = mineCount;
        mineCountText.text = mineCount.ToString();
    }

    private bool HasSecondColor;

    private ColorType SecondColorType;

    public bool GetHasSecondColor()
    {
        return HasSecondColor;
    }

    public ColorType GetSecondColorType()
    {
        return SecondColorType;
    }

    public void SetHasSecondColor(bool hasSecondColor)
    {
        HasSecondColor = hasSecondColor;
        if (SecondColorContainer != null)
            SecondColorContainer.gameObject.SetActive(hasSecondColor);
    }

    public void SetSecondColorType(ColorType secondColorType)
    {
        SecondColorType = secondColorType;
        if (SecondColorSprites != null)
        {
            foreach (var spriteRenderer in SecondColorSprites)
            {
                spriteRenderer.color = ColorManager.GetColor(secondColorType);
            }
        }
    }
}
