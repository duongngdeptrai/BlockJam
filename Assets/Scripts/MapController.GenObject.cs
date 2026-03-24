using UnityEngine;

public partial class MapController : MonoBehaviour
{
    private void GenWalls()
    {
        if (currentLevelData == null || currentLevelData.walls == null || currentLevelData.walls.Count == 0)
        {
            return;
        }

        foreach (var wallData in currentLevelData.walls)
        {
            if (wallPrefab == null)
            {
                continue;
            }

            Wall wall = Instantiate(wallPrefab, wallContainer);
            
            Vector3 worldPos = CalculateWorldPosition(wallData.row, wallData.column, wallData.direction);
            wall.transform.position = transform.position + worldPos;
            
            Quaternion rotation = DirectionToRotation(wallData.direction);
            wall.transform.localRotation = rotation;
            
            listWall.Add(wall);
        }
    }

    private void GenDoors()
    {
        if (currentLevelData == null || currentLevelData.doors == null || currentLevelData.doors.Count == 0)
        {
            return;
        }

        foreach (var doorData in currentLevelData.doors)
        {
            if (!doorPrefabDict.ContainsKey(doorData.doorType))
            {
                continue;
            }

            Door doorPrefab = doorPrefabDict[doorData.doorType];
            Door door = Instantiate(doorPrefab, doorContainer);
            
            Vector3 worldPos = CalculateWorldPosition(doorData.row, doorData.column, doorData.direction);
            door.transform.position = transform.position + worldPos;
            
            Quaternion rotation = DirectionToRotation(doorData.direction);
            door.transform.localRotation = rotation;
            
            ColorType colorType = GetColorTypeFromString(doorData.color);
            door.UpdateColorImg(colorType);
            
            listDoor.Add(door);

            door.Direction = doorData.direction switch
            {
                "Down" => Direction.Down,
                "Up" => Direction.Up,
                "Left" => Direction.Left,
                "Right" => Direction.Right,
                _ => Direction.Up
            };
        }
    }
    private void GenBlocks()
    {
        if (currentLevelData == null || currentLevelData.blocks == null)
        {
            return;
        }

        foreach (var blockData in currentLevelData.blocks)
        {
            if (!blockPrefabDict.ContainsKey(blockData.blockType))
            {
                continue;
            }
            Block blockPrefab = blockPrefabDict[blockData.blockType];
            Block block = Instantiate(blockPrefab, blockContainer);

            block.transform.position = transform.position + new Vector3(
                blockData.column - (columns - 1) / 2f,
                -(blockData.row - (rows - 1) / 2f),
                0
            );
            if (System.Enum.TryParse<ColorType>(blockData.color, out ColorType colorType))
            {
                block.SetColorBlock(colorType);
            }
            
            listBlock.Add(block);
        }
    }
    private void GenGrid()
    {
        for(int i = 0; i < rows; i++)
        {
            for(int j = 0; j < columns; j++)
            {
                var grid = Instantiate(Grid, gridContainer);
                grid.transform.position = transform.position + new Vector3(j - (columns - 1) / 2f, i - (rows - 1) / 2f, 0);
            }
        }
    }

    private Vector3 CalculateWorldPosition(int row, int column, string Direction )
    {
        Vector3 pos =  new Vector3(
            +(column - (columns - 1) / 2f),
            -(row - (rows - 1) / 2f),
            0
        );
        switch (Direction)
        {
            case "Down":
                return pos + new Vector3(0, -0.5f, 0);
            case "Up":
                return pos + new Vector3(0, +0.5f, 0);
            case "Left":
                return pos + new Vector3(-0.5f, 0, 0);
            case "Right":
                return pos + new Vector3(0.5f, 0, 0);
            default:
                return pos;
        }
    }

    private Quaternion DirectionToRotation(string direction)
    {
        switch (direction)
        {
            case "Down":
                return Quaternion.Euler(0, 180, 180);
            case "Up":
                return Quaternion.Euler(0, 0, 0);
            case "Left":
                return Quaternion.Euler(180, 0, 90);
            case "Right":
                return Quaternion.Euler(0, 0, 270);
            default:
                return Quaternion.Euler(0, 0, 0);
        }
    }

    private ColorType GetColorTypeFromString(string colorString)
    {
        if (System.Enum.TryParse<ColorType>(colorString, out ColorType colorType))
        {
            return colorType;
        }
        
        return ColorType.None;
    }
}
