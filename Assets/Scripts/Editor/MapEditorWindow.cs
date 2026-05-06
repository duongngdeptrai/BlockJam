using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class MapEditorWindow : EditorWindow
{
    private int rows = 7;
    private int columns = 14;
    private float timeLimit = 120f;
    
    // Blocks
    private List<BlockData> blocks = new List<BlockData>();
    private Vector2 blockScrollPos;
    private string selectedBlockType = "block1x1";
    private int blockRow = 0;
    private int blockColumn = 0;
    private string blockColor = "Red";
    
    // Doors
    private List<DoorSpawnData> doors = new List<DoorSpawnData>();
    private Vector2 doorScrollPos;
    private int doorRow = 0;
    private int doorColumn = 0;
    private string doorDirection = "Up";
    private string doorType = "door1";
    private string doorColor = "Red";
    
    // Walls
    private List<WallSpawnData> walls = new List<WallSpawnData>();
    private Vector2 wallScrollPos;
    private int wallRow = 0;
    private int wallColumn = 0;
    private string wallDirection = "Up";
    
    // Save/Load
    private string levelName = "level";
    private GUIStyle headerStyle;
    private GUIStyle boxStyle;
    private bool stylesInitialized = false;
    private Vector2 mainScrollPos;
    private bool showPreview = true;
    private bool blocksExpanded = true;
    private bool doorsExpanded = true;
    private bool wallsExpanded = true;
    private bool blockCreateExpanded = true;
    private bool doorCreateExpanded = true;
    private bool wallCreateExpanded = true;
    
    private string[] blockTypes = { "block1x1", "block1x2", "block1x3", "block1x4", "block2x2", "block2x3", "blockLShape", "blockTShape", "blockZShape" };
    private string[] colors = { "Red", "Green", "Blue", "Yellow", "Purple", "Orange", "None" };
    private string[] directions = { "Up", "Down", "Left", "Right" };
    private string[] doorTypes = { "door1", "door2", "door3", "door4" };
    
    [MenuItem("Window/PikaGame/Map Editor")]
    public static void ShowWindow()
    {
        GetWindow<MapEditorWindow>("Map Editor");
    }
    
    private void InitializeStyles()
    {
        if (stylesInitialized) return;
        
        headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(5, 5, 5, 5)
        };
        
        boxStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(10, 10, 10, 10),
            margin = new RectOffset(5, 5, 5, 5)
        };
        
        stylesInitialized = true;
    }
    
    private void OnGUI()
    {
        // Ensure styles are initialized
        InitializeStyles();
        
        mainScrollPos = GUILayout.BeginScrollView(mainScrollPos);
        GUILayout.BeginVertical();
        
        // Title
        GUILayout.Label("PikaGame Map Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // Preview Toggle
        EditorGUILayout.BeginHorizontal();
        showPreview = GUILayout.Toggle(showPreview, "Show Map Preview", GUILayout.Width(150));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5);
        
        // Preview Section
        if (showPreview)
        {
            DrawMapPreview();
            EditorGUILayout.Space(10);
        }
        
        // Map Settings
        DrawMapSettings();
        EditorGUILayout.Space(10);
        
        // Blocks Section
        DrawBlocksSection();
        EditorGUILayout.Space(10);
        
        // Doors Section
        DrawDoorsSection();
        EditorGUILayout.Space(10);
        
        // Walls Section
        DrawWallsSection();
        EditorGUILayout.Space(10);
        
        // Save/Load Section
        DrawSaveLoadSection();
        
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
    }
    
    private void DrawMapPreview()
    {
        EditorGUILayout.LabelField("Map Preview", headerStyle);
        EditorGUILayout.BeginVertical(boxStyle);
        
        const int labelSize = 18;
        int cellSize = 24;
        int previewColumns = columns + 2;
        int previewRows = rows + 2;
        int totalWidth = labelSize + previewColumns * cellSize;
        int totalHeight = labelSize + previewRows * cellSize;

        Rect previewArea = GUILayoutUtility.GetRect(totalWidth, totalHeight);
        EditorGUI.DrawRect(previewArea, new Color(0.12f, 0.12f, 0.12f));

        Rect gridOrigin = new Rect(previewArea.x + labelSize, previewArea.y + labelSize, previewColumns * cellSize, previewRows * cellSize);
        Color borderColor = new Color(0.28f, 0.28f, 0.28f);
        Color innerColor = new Color(0.18f, 0.18f, 0.18f);
        Color outerColor = new Color(0.10f, 0.10f, 0.10f);

        for (int visualRow = 0; visualRow < previewRows; visualRow++)
        {
            int mapRow = visualRow - 1;
            for (int visualColumn = 0; visualColumn < previewColumns; visualColumn++)
            {
                int mapColumn = visualColumn - 1;
                Rect cellRect = new Rect(
                    gridOrigin.x + visualColumn * cellSize,
                    gridOrigin.y + visualRow * cellSize,
                    cellSize,
                    cellSize);

                bool insidePlayable = IsInsidePlayableArea(mapRow, mapColumn);
                EditorGUI.DrawRect(cellRect, insidePlayable ? innerColor : outerColor);
                DrawCellBorder(cellRect, borderColor);
            }
        }

        GUIStyle indexStyle = EditorStyles.centeredGreyMiniLabel;
        for (int visualColumn = 0; visualColumn < previewColumns; visualColumn++)
        {
            int mapColumn = visualColumn - 1;
            Rect labelRect = new Rect(
                gridOrigin.x + visualColumn * cellSize,
                previewArea.y,
                cellSize,
                labelSize);
            GUI.Label(labelRect, mapColumn.ToString(), indexStyle);
        }

        for (int visualRow = 0; visualRow < previewRows; visualRow++)
        {
            int mapRow = visualRow - 1;
            Rect labelRect = new Rect(
                previewArea.x,
                gridOrigin.y + visualRow * cellSize,
                labelSize,
                cellSize);
            GUI.Label(labelRect, mapRow.ToString(), indexStyle);
        }

        foreach (var block in blocks)
        {
            DrawBlockPreview(block, gridOrigin, cellSize, borderColor);
        }

        foreach (var door in doors)
        {
            DrawDoorPreview(door, gridOrigin, cellSize);
        }
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField($"Map Size: {rows}x{columns} (preview shows -1 and {rows}/{columns} border cells)", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Blocks: {blocks.Count} | Doors: {doors.Count} | Walls: {walls.Count}", EditorStyles.miniLabel);
        
        EditorGUILayout.EndVertical();
    }

    private void DrawBlockPreview(BlockData block, Rect gridOrigin, int cellSize, Color borderColor)
    {
        if (block == null)
        {
            return;
        }

        List<Vector2Int> footprint = GetBlockFootprint(block.blockType);
        Color fillColor = GetColorFromString(block.color);

        for (int i = 0; i < footprint.Count; i++)
        {
            Vector2Int offset = footprint[i];
            int mapRow = block.row + offset.y;
            int mapColumn = block.column + offset.x;

            if (!IsInsidePreviewArea(mapRow, mapColumn))
            {
                continue;
            }

            Rect cellRect = GetPreviewCellRect(gridOrigin, cellSize, mapRow, mapColumn);
            EditorGUI.DrawRect(cellRect, fillColor);
            DrawCellBorder(cellRect, borderColor);
        }

        if (IsInsidePreviewArea(block.row, block.column))
        {
            Rect anchorRect = GetPreviewCellRect(gridOrigin, cellSize, block.row, block.column);
            GUI.Label(anchorRect, GetBlockShortLabel(block.blockType), EditorStyles.centeredGreyMiniLabel);
        }
    }

    private void DrawDoorPreview(DoorSpawnData door, Rect gridOrigin, int cellSize)
    {
        if (door == null)
        {
            return;
        }
        // Use explicit row/column from the door data so editor shows exact coordinates
        int mapRow = door.row;
        int mapColumn = door.column;

        if (!IsInsidePreviewArea(mapRow, mapColumn))
        {
            return;
        }

        Rect doorRect = GetPreviewCellRect(gridOrigin, cellSize, mapRow, mapColumn);
        Color baseColor = GetColorFromString(door.color);
        Color fillColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.70f);

        EditorGUI.DrawRect(doorRect, fillColor);
        DrawCellBorder(doorRect, Color.white);

        Rect innerRect = new Rect(doorRect.x + 3, doorRect.y + 3, doorRect.width - 6, doorRect.height - 6);
        EditorGUI.DrawRect(innerRect, new Color(0f, 0f, 0f, 0.18f));

        // Draw door type prominently and direction smaller below
        GUI.Label(doorRect, string.IsNullOrEmpty(door.doorType) ? GetDoorShortLabel(door) : door.doorType, EditorStyles.boldLabel);
        string dir = door.direction switch { "Up" => "^", "Down" => "v", "Left" => "<", "Right" => ">", _ => "?" };
        Rect dirRect = new Rect(doorRect.x, doorRect.y + doorRect.height * 0.5f, doorRect.width, doorRect.height * 0.5f);
        GUI.Label(dirRect, dir, EditorStyles.miniLabel);
    }

    private Rect GetPreviewCellRect(Rect gridOrigin, int cellSize, int mapRow, int mapColumn)
    {
        int visualRow = mapRow + 1;
        int visualColumn = mapColumn + 1;

        return new Rect(
            gridOrigin.x + visualColumn * cellSize,
            gridOrigin.y + visualRow * cellSize,
            cellSize,
            cellSize);
    }

    private bool IsInsidePreviewArea(int row, int column)
    {
        return row >= -1 && row <= rows && column >= -1 && column <= columns;
    }

    private bool IsInsidePlayableArea(int row, int column)
    {
        return row >= 0 && row < rows && column >= 0 && column < columns;
    }

    private void DrawCellBorder(Rect rect, Color borderColor)
    {
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), borderColor);
        EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), borderColor);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1, rect.height), borderColor);
        EditorGUI.DrawRect(new Rect(rect.xMax - 1, rect.y, 1, rect.height), borderColor);
    }

    private Vector2Int GetDoorPreviewCell(DoorSpawnData door)
    {
        int row = door.row;
        int column = door.column;

        return door.direction switch
        {
            "Up" => new Vector2Int(column, row - 1),
            "Down" => new Vector2Int(column, row + 1),
            "Left" => new Vector2Int(column - 1, row),
            "Right" => new Vector2Int(column + 1, row),
            _ => new Vector2Int(column, row)
        };
    }

    private List<Vector2Int> GetBlockFootprint(string blockType)
    {
        List<Vector2Int> footprint = new List<Vector2Int>();

        switch (blockType)
        {
            case "block1x1":
                footprint.Add(new Vector2Int(0, 0));
                break;
            case "block1x2":
                footprint.Add(new Vector2Int(0, 0));
                footprint.Add(new Vector2Int(0, 1));
                break;
            case "block1x3":
                footprint.Add(new Vector2Int(0, 0));
                footprint.Add(new Vector2Int(0, 1));
                footprint.Add(new Vector2Int(0, 2));
                break;
            case "block1x4":
                footprint.Add(new Vector2Int(0, 0));
                footprint.Add(new Vector2Int(0, 1));
                footprint.Add(new Vector2Int(0, 2));
                footprint.Add(new Vector2Int(0, 3));
                break;
            case "block2x2":
                footprint.Add(new Vector2Int(0, 0));
                footprint.Add(new Vector2Int(1, 0));
                footprint.Add(new Vector2Int(0, 1));
                footprint.Add(new Vector2Int(1, 1));
                break;
            case "block2x3":
                footprint.Add(new Vector2Int(0, 0));
                footprint.Add(new Vector2Int(1, 0));
                footprint.Add(new Vector2Int(0, 1));
                footprint.Add(new Vector2Int(1, 1));
                footprint.Add(new Vector2Int(0, 2));
                footprint.Add(new Vector2Int(1, 2));
                break;
            case "blockLShape":
                footprint.Add(new Vector2Int(0, 0));
                footprint.Add(new Vector2Int(0, 1));
                footprint.Add(new Vector2Int(0, 2));
                footprint.Add(new Vector2Int(1, 2));
                break;
            case "blockTShape":
                footprint.Add(new Vector2Int(0, 0));
                footprint.Add(new Vector2Int(1, 0));
                footprint.Add(new Vector2Int(2, 0));
                footprint.Add(new Vector2Int(1, 1));
                break;
            case "blockZShape":
                footprint.Add(new Vector2Int(0, 0));
                footprint.Add(new Vector2Int(1, 0));
                footprint.Add(new Vector2Int(1, 1));
                footprint.Add(new Vector2Int(2, 1));
                break;
            default:
                footprint.Add(new Vector2Int(0, 0));
                break;
        }

        return footprint;
    }

    private string GetBlockShortLabel(string blockType)
    {
        return blockType switch
        {
            "block1x1" => "1",
            "block1x2" => "2",
            "block1x3" => "3",
            "block1x4" => "4",
            "block2x2" => "2x2",
            "block2x3" => "2x3",
            "blockLShape" => "L",
            "blockTShape" => "T",
            "blockZShape" => "Z",
            _ => "?"
        };
    }

    private string GetDoorShortLabel(DoorSpawnData door)
    {
        string directionCode = door.direction switch
        {
            "Up" => "^",
            "Down" => "v",
            "Left" => "<",
            "Right" => ">",
            _ => "?"
        };

        return string.IsNullOrEmpty(door.doorType) ? directionCode : $"{door.doorType}\n{directionCode}";
    }
    
    private Color GetColorFromString(string colorName)
    {
        return colorName switch
        {
            "Red" => new Color(1f, 0.2f, 0.2f),
            "Green" => new Color(0.2f, 1f, 0.2f),
            "Blue" => new Color(0.2f, 0.2f, 1f),
            "Yellow" => new Color(1f, 1f, 0.2f),
            "Purple" => new Color(1f, 0.2f, 1f),
            "Orange" => new Color(1f, 0.6f, 0.2f),
            "None" => new Color(0.5f, 0.5f, 0.5f),
            _ => Color.white
        };
    }
    
    private void DrawMapSettings()
    {
        EditorGUILayout.LabelField("Map Settings", headerStyle);
        
        EditorGUILayout.BeginVertical(boxStyle);
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Rows", GUILayout.Width(100));
        rows = EditorGUILayout.IntSlider(rows, 5, 20);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Columns", GUILayout.Width(100));
        columns = EditorGUILayout.IntSlider(columns, 10, 20);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Time Limit", GUILayout.Width(100));
        timeLimit = EditorGUILayout.FloatField(timeLimit);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawBlocksSection()
    {
        EditorGUILayout.LabelField("Blocks", headerStyle);
        
        EditorGUILayout.BeginVertical(boxStyle);
        blocksExpanded = EditorGUILayout.Foldout(blocksExpanded, $"Blocks ({blocks.Count})", true);

        if (blocksExpanded)
        {
            blockCreateExpanded = EditorGUILayout.Foldout(blockCreateExpanded, "Add Block", true);
            if (blockCreateExpanded)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Type", GUILayout.Width(60));
                int typeIndex = GetPopupIndex(blockTypes, selectedBlockType);
                typeIndex = EditorGUILayout.Popup(typeIndex, blockTypes, GUILayout.Width(120));
                selectedBlockType = blockTypes[typeIndex];
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Row", GUILayout.Width(60));
                blockRow = EditorGUILayout.IntField(blockRow, GUILayout.Width(60));
                GUILayout.Label("Column", GUILayout.Width(60));
                blockColumn = EditorGUILayout.IntField(blockColumn, GUILayout.Width(60));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Color", GUILayout.Width(60));
                int colorIndex = GetPopupIndex(colors, blockColor);
                colorIndex = EditorGUILayout.Popup(colorIndex, colors, GUILayout.Width(120));
                blockColor = colors[colorIndex];
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Add Block", GUILayout.Height(26)))
                {
                    AddBlock();
                }
            }

            EditorGUILayout.Space(4);

            blockScrollPos = EditorGUILayout.BeginScrollView(blockScrollPos, GUILayout.Height(170));
            for (int i = 0; i < blocks.Count; i++)
            {
                BlockData block = blocks[i];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"#{i}", GUILayout.Width(28));

                int blockTypeIndex = GetPopupIndex(blockTypes, block.blockType);
                blockTypeIndex = EditorGUILayout.Popup(blockTypeIndex, blockTypes, GUILayout.Width(110));
                block.blockType = blockTypes[blockTypeIndex];

                block.row = EditorGUILayout.IntField(block.row, GUILayout.Width(45));
                block.column = EditorGUILayout.IntField(block.column, GUILayout.Width(45));

                int blockColorIndex = GetPopupIndex(colors, block.color);
                blockColorIndex = EditorGUILayout.Popup(blockColorIndex, colors, GUILayout.Width(90));
                block.color = colors[blockColorIndex];

                if (GUILayout.Button("X", GUILayout.Width(24)))
                {
                    blocks.RemoveAt(i);
                    i--;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    continue;
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear All Blocks"))
            {
                blocks.Clear();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawDoorsSection()
    {
        EditorGUILayout.LabelField("Doors", headerStyle);
        
        EditorGUILayout.BeginVertical(boxStyle);
        doorsExpanded = EditorGUILayout.Foldout(doorsExpanded, $"Doors ({doors.Count})", true);

        if (doorsExpanded)
        {
            doorCreateExpanded = EditorGUILayout.Foldout(doorCreateExpanded, "Add Door", true);
            if (doorCreateExpanded)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Row", GUILayout.Width(60));
                doorRow = EditorGUILayout.IntField(doorRow, GUILayout.Width(60));
                GUILayout.Label("Column", GUILayout.Width(60));
                doorColumn = EditorGUILayout.IntField(doorColumn, GUILayout.Width(60));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Direction", GUILayout.Width(80));
                int dirIndex = GetPopupIndex(directions, doorDirection);
                dirIndex = EditorGUILayout.Popup(dirIndex, directions, GUILayout.Width(100));
                doorDirection = directions[dirIndex];
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Type", GUILayout.Width(60));
                int typeIndex = GetPopupIndex(doorTypes, doorType);
                typeIndex = EditorGUILayout.Popup(typeIndex, doorTypes, GUILayout.Width(100));
                doorType = doorTypes[typeIndex];
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Color", GUILayout.Width(60));
                int colorIndex = GetPopupIndex(colors, doorColor);
                colorIndex = EditorGUILayout.Popup(colorIndex, colors, GUILayout.Width(100));
                doorColor = colors[colorIndex];
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Add Door", GUILayout.Height(26)))
                {
                    AddDoor();
                }
            }

            EditorGUILayout.Space(4);

            doorScrollPos = EditorGUILayout.BeginScrollView(doorScrollPos, GUILayout.Height(170));
            for (int i = 0; i < doors.Count; i++)
            {
                DoorSpawnData door = doors[i];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"#{i}", GUILayout.Width(28));

                door.row = EditorGUILayout.IntField(door.row, GUILayout.Width(45));
                door.column = EditorGUILayout.IntField(door.column, GUILayout.Width(45));

                int doorDirectionIndex = GetPopupIndex(directions, door.direction);
                doorDirectionIndex = EditorGUILayout.Popup(doorDirectionIndex, directions, GUILayout.Width(80));
                door.direction = directions[doorDirectionIndex];

                int doorTypeIndex = GetPopupIndex(doorTypes, door.doorType);
                doorTypeIndex = EditorGUILayout.Popup(doorTypeIndex, doorTypes, GUILayout.Width(75));
                door.doorType = doorTypes[doorTypeIndex];

                int doorColorIndex = GetPopupIndex(colors, door.color);
                doorColorIndex = EditorGUILayout.Popup(doorColorIndex, colors, GUILayout.Width(90));
                door.color = colors[doorColorIndex];

                if (GUILayout.Button("X", GUILayout.Width(24)))
                {
                    doors.RemoveAt(i);
                    i--;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    continue;
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();

            if (doors.Count > 0 && GUILayout.Button("Clear All Doors"))
            {
                doors.Clear();
            }
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawWallsSection()
    {
        EditorGUILayout.LabelField("Walls", headerStyle);
        
        EditorGUILayout.BeginVertical(boxStyle);
        wallsExpanded = EditorGUILayout.Foldout(wallsExpanded, $"Walls ({walls.Count})", true);

        if (wallsExpanded)
        {
            wallCreateExpanded = EditorGUILayout.Foldout(wallCreateExpanded, "Add Wall", true);
            if (wallCreateExpanded)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Row", GUILayout.Width(60));
                wallRow = EditorGUILayout.IntField(wallRow, GUILayout.Width(60));
                GUILayout.Label("Column", GUILayout.Width(60));
                wallColumn = EditorGUILayout.IntField(wallColumn, GUILayout.Width(60));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Direction", GUILayout.Width(80));
                int dirIndex = GetPopupIndex(directions, wallDirection);
                dirIndex = EditorGUILayout.Popup(dirIndex, directions, GUILayout.Width(100));
                wallDirection = directions[dirIndex];
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Add Wall", GUILayout.Height(26)))
                {
                    AddWall();
                }
            }

            EditorGUILayout.Space(4);

            wallScrollPos = EditorGUILayout.BeginScrollView(wallScrollPos, GUILayout.Height(170));
            for (int i = 0; i < walls.Count; i++)
            {
                WallSpawnData wall = walls[i];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"#{i}", GUILayout.Width(28));

                wall.row = EditorGUILayout.IntField(wall.row, GUILayout.Width(45));
                wall.column = EditorGUILayout.IntField(wall.column, GUILayout.Width(45));

                int wallDirectionIndex = GetPopupIndex(directions, wall.direction);
                wallDirectionIndex = EditorGUILayout.Popup(wallDirectionIndex, directions, GUILayout.Width(80));
                wall.direction = directions[wallDirectionIndex];

                if (GUILayout.Button("X", GUILayout.Width(24)))
                {
                    walls.RemoveAt(i);
                    i--;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    continue;
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();

            if (walls.Count > 0 && GUILayout.Button("Clear All Walls"))
            {
                walls.Clear();
            }
        }
        
        EditorGUILayout.EndVertical();
    }

    private int GetPopupIndex(string[] options, string currentValue)
    {
        int index = System.Array.IndexOf(options, currentValue);
        return index < 0 ? 0 : index;
    }
    
    private void DrawSaveLoadSection()
    {
        EditorGUILayout.LabelField("Save / Load", headerStyle);
        
        EditorGUILayout.BeginVertical(boxStyle);
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Level Name", GUILayout.Width(100));
        levelName = EditorGUILayout.TextField(levelName);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Save Map", GUILayout.Height(35)))
        {
            SaveMap();
        }
        
        if (GUILayout.Button("Load Map", GUILayout.Height(35)))
        {
            LoadMap();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("New Map", GUILayout.Height(30)))
        {
            NewMap();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void AddBlock()
    {
        BlockData newBlock = new BlockData
        {
            blockType = selectedBlockType,
            row = blockRow,
            column = blockColumn,
            color = blockColor
        };
        blocks.Add(newBlock);
    }
    
    private void AddDoor()
    {
        DoorSpawnData newDoor = new DoorSpawnData
        {
            row = doorRow,
            column = doorColumn,
            direction = doorDirection,
            doorType = doorType,
            color = doorColor
        };
        doors.Add(newDoor);
    }
    
    private void AddWall()
    {
        WallSpawnData newWall = new WallSpawnData
        {
            row = wallRow,
            column = wallColumn,
            direction = wallDirection
        };
        walls.Add(newWall);
    }
    
    private void SaveMap()
    {
        if (string.IsNullOrEmpty(levelName))
        {
            EditorUtility.DisplayDialog("Error", "Please enter a level name", "OK");
            return;
        }
        
        LevelData levelData = new LevelData
        {
            rows = rows,
            columns = columns,
            timeLimit = timeLimit,
            blocks = new List<BlockData>(blocks),
            doors = new List<DoorSpawnData>(doors),
            walls = new List<WallSpawnData>(walls)
        };
        
        LevelDataWrapper wrapper = new LevelDataWrapper { level = levelData };
        string json = JsonUtility.ToJson(wrapper, true);
        
        string levelsPath = Path.Combine(Application.streamingAssetsPath, "Levels");
        if (!Directory.Exists(levelsPath))
        {
            Directory.CreateDirectory(levelsPath);
        }
        
        string filePath = Path.Combine(levelsPath, levelName + ".json");
        File.WriteAllText(filePath, json);
        
        EditorUtility.DisplayDialog("Success", $"Map saved as {levelName}.json", "OK");
    }
    
    private void LoadMap()
    {
        if (string.IsNullOrEmpty(levelName))
        {
            EditorUtility.DisplayDialog("Error", "Please enter a level name", "OK");
            return;
        }
        
        string levelsPath = Path.Combine(Application.streamingAssetsPath, "Levels");
        string filePath = Path.Combine(levelsPath, levelName + ".json");
        
        if (!File.Exists(filePath))
        {
            EditorUtility.DisplayDialog("Error", $"File {levelName}.json not found", "OK");
            return;
        }
        
        string json = File.ReadAllText(filePath);
        LevelDataWrapper wrapper = JsonUtility.FromJson<LevelDataWrapper>(json);
        LevelData levelData = wrapper.level;
        
        rows = levelData.rows;
        columns = levelData.columns;
        timeLimit = levelData.timeLimit;
        blocks = new List<BlockData>(levelData.blocks);
        doors = new List<DoorSpawnData>(levelData.doors);
        walls = new List<WallSpawnData>(levelData.walls);
        
        EditorUtility.DisplayDialog("Success", $"Map {levelName}.json loaded", "OK");
    }
    
    private void NewMap()
    {
        if (EditorUtility.DisplayDialog("New Map", "Create a new map? Unsaved changes will be lost.", "Yes", "No"))
        {
            rows = 7;
            columns = 14;
            timeLimit = 120f;
            blocks.Clear();
            doors.Clear();
            walls.Clear();
            levelName = "level";
        }
    }
}
