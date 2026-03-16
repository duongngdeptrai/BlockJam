using System;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] private int typeImg = 0;
    [SerializeField] private GameObject frontImgObject;
    [SerializeField] private SpriteRenderer Img;

    public int TypeImg { get => typeImg; set => typeImg = value; }

    private bool isClicked = false;

    public void SetColorBlock(ColorType colorType)
    {
        Img.color = GetColor(colorType);
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
}
