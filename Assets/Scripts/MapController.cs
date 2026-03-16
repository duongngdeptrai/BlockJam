using UnityEngine;

public class MapController : MonoBehaviour
{
    [SerializeField] private int rows = 7, columns = 14;
    [SerializeField] private Door doorPrefab;
    [SerializeField] private Wall wallPrefab;
    [SerializeField] private Block blockPrefab;
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
    private int[,] mapBlockData =
    {
        {0,0,0,0,0,0,0,0,0,0,0,0,0,0},
        {0,2,3,4,0,1,0,1,5,4,1,1,1,0},
        {0,1,0,0,0,0,0,0,0,0,0,0,1,0},
        {0,1,0,1,1,1,1,1,1,1,1,0,1,0},
        {0,1,0,1,0,0,0,0,0,0,1,0,1,0},
        {0,1,0,1,0,0,0,0,0,0,1,0,1,0},
        {0,1,0,1,0,0,0,0,0,0,1,0,1,0},
    };                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          
    private void Start()
    {
        GenWalls();
        GenDoors();
        GenBlocks();
    }
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
                wallRight.transform.localRotation = Quaternion.Euler(0, 0, 90); 
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
            }

            if(mapBorderData[rows - 1, j] == 0)
            {
                var wallBottom = Instantiate(wallPrefab, transform);
                wallBottom.transform.position = transform.position - new Vector3(0, offsetY, 0);
                wallBottom.transform.position += new Vector3(offsetX, 0, 0);
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
        for(int i = 0; i < rows; i++)
        {
            for(int j = 0; j < columns; j++)
            {
                if(mapBlockData[i, j] != 0)
                {
                    var block = Instantiate(blockPrefab, transform);
                    block.transform.position = transform.position + new Vector3(j - (columns - 1) / 2f, i - (rows - 1) / 2f, 0);
                    block.SetColorBlock(GetColorType(mapBlockData[i, j]));
                }
            }
        }
    }
    private ColorType GetColorType(int value)
    {
        return (ColorType)value;
    }
}
