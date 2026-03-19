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
public class DoorData
{
    public int row;
    public int column;
    public string color;
    public string position; // chưa xong đừng động vào cái doorData này nhé, nó còn đang để tạm đó để test thôi, sau này sẽ sửa lại cho hợp lý hơn
}

[System.Serializable]
public class LevelData
{
    public int rows;
    public int columns;
    public List<BlockData> blocks = new List<BlockData>();
    public List<DoorData> doors = new List<DoorData>();
}

[System.Serializable]
public class LevelDataWrapper
{
    public LevelData level;
}
