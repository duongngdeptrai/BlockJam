using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlockData
{
public string blockType;
public int row;
public int column;
public string color;
public bool hasMine;
public int mineCount;
public bool hasSecondColor;
public string secondColor;
public string direction; // "Up", "Down", "Left", "Right" — default "Up"
}

[System.Serializable]
public class DoorSpawnData
{
public int row;
public int column;
public string direction; // "Up", "Down", "Left", "Right"
public string doorType; // "door1", "door2", "door3", "door4"
public string color; // "Red", "Green", "Blue", "Yellow", "Purple", "Orange", "None"
}

[System.Serializable]
public class WallSpawnData
{
public int row;
public int column;
public string direction; // "Up", "Down", "Left", "Right"
}

[System.Serializable]
public class LevelData
{
public int rows;
public int columns;
public float timeLimit = 60f; // Thời gian giới hạn của level (tính bằng giây)
public List<BlockData> blocks = new List<BlockData>();
public List<DoorSpawnData> doors = new List<DoorSpawnData>();
public List<WallSpawnData> walls = new List<WallSpawnData>();
}

[System.Serializable]
public class LevelDataWrapper
{
public LevelData level;
}
