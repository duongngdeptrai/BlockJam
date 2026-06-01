using System;
using System.Drawing;
using TMPro;
using UnityEngine;

public abstract class Block : MonoBehaviour
{
    void Awake()
    {
        // SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();

        // foreach (var img in sprites)
        // {
        //     img.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        // }
    }
    [SerializeField] protected SpriteRenderer[] Img;
    [SerializeField] protected int sizeX = 1, sizeY = 1;
    [SerializeField] protected Transform MineContainer;
    [SerializeField] protected TextMeshPro mineCountText;

    public ColorType colorType; // Lưu giữ data màu thật 

    public int SizeX { get => sizeX; }
    public int SizeY { get => sizeY; }
    public ColorType ColorType { get => colorType; }

    private bool HasMine;

    private int MineCount;

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
}
