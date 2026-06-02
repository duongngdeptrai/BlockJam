using UnityEngine;
public partial class MapController : MonoBehaviour
{
	private Transform EnsureContainerParent(Transform container, string name)
	{
		if (container == null)
		{
			container = new GameObject(name).transform;
			container.SetParent(mapRoot, false);
		}
		else if (container.parent != mapRoot)
		{
			container.SetParent(mapRoot, false);
		}
		return container;
	}

	private void GenWalls()
	{
		if (currentLevelData == null || currentLevelData.walls == null || currentLevelData.walls.Count == 0) return;

		wallContainer = EnsureContainerParent(wallContainer, "Walls");

		foreach (var wallData in currentLevelData.walls)
		{
			if (wallPrefab == null)
			{
				Debug.LogError("wallPrefab is null! Assign a Wall prefab in MapController Inspector.");
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
		if (currentLevelData == null || currentLevelData.doors == null || currentLevelData.doors.Count == 0) return;

		if (doorPrefabDict.Count == 0)
		{
			Debug.LogError("doorPrefabDict is empty! Check MapController Door Prefabs in Inspector.");
			return;
		}

		doorContainer = EnsureContainerParent(doorContainer, "Doors");

		foreach (var doorData in currentLevelData.doors)
		{
			string key = doorData.doorType;

			if (!doorPrefabDict.TryGetValue(key, out Door doorPrefab))
			{
				Debug.LogError($"Door prefab not found for key '{key}'. Available keys: [{string.Join(", ", doorPrefabDict.Keys)}]");
				continue;
			}

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
		if (currentLevelData == null || currentLevelData.blocks == null || currentLevelData.blocks.Count == 0) return;

		if (blockPrefabDict.Count == 0)
		{
			Debug.LogError("blockPrefabDict is empty! Check MapController Block Prefabs in Inspector.");
			return;
		}

		blockContainer = EnsureContainerParent(blockContainer, "Blocks");

		foreach (var blockData in currentLevelData.blocks)
		{
			string key = blockData.blockType;

			if (!blockPrefabDict.TryGetValue(key, out Block blockPrefab))
			{
				Debug.LogError($"Block prefab not found for key '{key}'. Available keys: [{string.Join(", ", blockPrefabDict.Keys)}]");
				continue;
			}

			Block block = Instantiate(blockPrefab, blockContainer);
			block.transform.position = transform.position + new Vector3(
				blockData.column - (columns - 1) / 2f,
				-(blockData.row - (rows - 1) / 2f),
				0);
			ColorType colorType = GetColorTypeFromString(blockData.color);
			block.SetColorBlock(colorType);
			block.SetHasMine(blockData.hasMine);
			if (blockData.hasMine)
			{
				block.SetMineCount(blockData.mineCount);
			}
			listBlock.Add(block);
		}
	}

	private void GenGrid()
	{
		if (currentLevelData == null) return;

		gridContainer = EnsureContainerParent(gridContainer, "GridCells");

		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < columns; j++)
			{
				if (Grid == null)
				{
					Debug.LogError("Grid prefab is null! Assign a Grid prefab in MapController Inspector.");
					return;
				}
				var grid = Instantiate(Grid, gridContainer);
				grid.transform.position = transform.position + new Vector3(
					j - (columns - 1) / 2f,
					i - (rows - 1) / 2f,
					0);
			}
		}
	}
}
