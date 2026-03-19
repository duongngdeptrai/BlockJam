using UnityEngine;

public partial class MapController : MonoBehaviour
{
    private int[,] mapBorderData =
    {
        {0,1,0,0,2,0,1,0,4,0,0,1,0,1},
        {0,0,0,0,0,0,0,0,0,0,0,0,0,0},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0,0,0,0,0,3},
        {4,0,0,0,0,0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {0,1,0,1,0,5,0,0,1,0,1,0,0,0},
    };    
    private void GenWalls()
    {
        for (int i = 0; i < rows; i++)
        {
            var offsetX = columns / 2f;
            var offsetY = i - (rows - 1) / 2f;

            if(mapBorderData[i, 0] == 0)
            {
                var wallLeft = Instantiate(wallPrefab, transform);
                wallLeft.transform.position = transform.position - new Vector3(offsetX, 0, 0);
                wallLeft.transform.position += new Vector3(0, offsetY, 0);
                wallLeft.transform.localRotation = Quaternion.Euler(0, 0, 90);  
            }
            
            if(mapBorderData[i, columns - 1] == 0)
            {
                var wallRight = Instantiate(wallPrefab, transform);
                wallRight.transform.position = transform.position + new Vector3(offsetX, 0, 0);
                wallRight.transform.position += new Vector3(0, offsetY, 0);
                wallRight.transform.localRotation = Quaternion.Euler(0, 0, 270); 
            }
        }
        for (int j = 0; j < columns; j++)
        {
            var offsetX = j - (columns - 1) / 2f;
            var offsetY = rows / 2f; 

            if(mapBorderData[0, j] == 0)
            {
                var wallTop = Instantiate(wallPrefab, transform);
                wallTop.transform.position = transform.position + new Vector3(0, offsetY, 0);
                wallTop.transform.position += new Vector3(offsetX, 0, 0);
                wallTop.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }

            if(mapBorderData[rows - 1, j] == 0)
            {
                var wallBottom = Instantiate(wallPrefab, transform);
                wallBottom.transform.position = transform.position - new Vector3(0, offsetY, 0);
                wallBottom.transform.position += new Vector3(offsetX, 0, 0);
                wallBottom.transform.localRotation = Quaternion.Euler(0, 0, 180);
            }
        }
    }
    private void GenDoors()
    {
        for(int i = 0; i < rows; i++)
        {
            if(mapBorderData[i, 0] != 0)
            {
                var door = Instantiate(doorPrefab, transform);
                door.transform.position = transform.position - new Vector3(columns / 2f, 0, 0);
                door.transform.position += new Vector3(0, i - (rows - 1) / 2f, 0);
                door.transform.localRotation = Quaternion.Euler(0, 0, 90);
                door.UpdateColorImg(GetColorType(mapBorderData[i, 0]));
            }
            if(mapBorderData[i, columns - 1] != 0)
            {
                var door = Instantiate(doorPrefab, transform);
                door.transform.position = transform.position + new Vector3(columns / 2f, 0, 0);
                door.transform.position += new Vector3(0, i - (rows - 1) / 2f, 0);
                door.transform.localRotation = Quaternion.Euler(0, 0, 270);
                door.UpdateColorImg(GetColorType(mapBorderData[i, columns - 1]));
            }
        }
        for(int j = 0; j < columns; j++)
        {
            if(mapBorderData[0, j] != 0)
            {
                var door = Instantiate(doorPrefab, transform);
                door.transform.position = transform.position + new Vector3(j - (columns - 1) / 2f, rows / 2f, 0);
                door.transform.localRotation = Quaternion.Euler(0, 0, 0);
                door.UpdateColorImg(GetColorType(mapBorderData[0, j]));
            }
            if(mapBorderData[rows - 1, j] != 0)
            {
                var door = Instantiate(doorPrefab, transform);
                door.transform.position = transform.position + new Vector3(j - (columns - 1) / 2f, -rows / 2f, 0);
                door.transform.localRotation = Quaternion.Euler(0, 0, 180);
                door.UpdateColorImg(GetColorType(mapBorderData[rows - 1, j]));
            }
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
            Block block = Instantiate(blockPrefab, transform);

            block.transform.position = transform.position + new Vector3(
                blockData.column - (columns - 1) / 2f,
                blockData.row - (rows - 1) / 2f,
                0
            );
            if (System.Enum.TryParse<ColorType>(blockData.color, out ColorType colorType))
            {
                block.SetColorBlock(colorType);
            }
        }
    }
    private void GenGrid()
    {
        for(int i = 0; i < rows; i++)
        {
            for(int j = 0; j < columns; j++)
            {
                var grid = Instantiate(Grid, transform);
                grid.transform.position = transform.position + new Vector3(j - (columns - 1) / 2f, i - (rows - 1) / 2f, 0);
            }
        }
    }
    private ColorType GetColorType(int value)
    {
        return (ColorType)value;
    }
}

[System.Serializable]
public class BlockPrefabEntry
{
    public string key;
    public Block prefab;
}
