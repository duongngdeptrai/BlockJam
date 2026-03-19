using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlockData
{
    public string blockType;
    public int row;
    public int column;
    public string color;
}

[System.Serializable]
public class LevelData
{
    public int rows;
    public int columns;
    public List<BlockData> blocks = new List<BlockData>();
}

[System.Serializable]
public class LevelDataWrapper
{
    public LevelData level;
}
