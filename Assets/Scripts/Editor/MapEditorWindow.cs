using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class MapEditorWindow : EditorWindow
{
    private int rows = 7;
    private int columns = 14;
    private float timeLimit = 120f;
    private List<BlockData> blocks = new List<BlockData>();
    private List<DoorSpawnData> doors = new List<DoorSpawnData>();
    private List<WallSpawnData> walls = new List<WallSpawnData>();

    private enum Tool { Select, PlaceBlock, PlaceDoor, PlaceWall, Eraser }
    private Tool currentTool = Tool.PlaceBlock;

    private string selectedBlockType = "block1x1";
    private string selectedColor = "Red";
    private bool placeBlockHasMine = false;
    private int placeBlockMineCount = 1;
    private bool placeBlockHasSecondColor = false;
    private string placeBlockSecondColor = "Red";

    private string doorDirection = "Up";
    private string doorType = "door1";
    private string doorColor = "Red";

    private string wallDirection = "Up";

    private int selectedBlockIndex = -1;
    private int selectedDoorIndex = -1;
    private int selectedWallIndex = -1;
    private bool hasSelection => selectedBlockIndex >= 0 || selectedDoorIndex >= 0 || selectedWallIndex >= 0;

    private bool isMouseDown = false;
    private Vector2Int? hoverCell = null;
    private Vector2Int? dragStartCell = null;

    private CommandHistory commandHistory = new CommandHistory();
    private Vector2 scrollPos;
    private bool showPreview = true;
    private string levelName = "level";

    private string[] blockTypes = { "block1x1", "block1x2", "block1x3", "block1x4", "block2x2", "block2x3", "blockLShape", "blockTShape", "blockZShape" };
    private string[] colors = { "Red", "Green", "Blue", "Yellow", "Purple", "Orange", "None" };
    private string[] directions = { "Up", "Down", "Left", "Right" };
    private string[] doorTypes = { "door1", "door2", "door3", "door4" };

    private GUIStyle headerStyle;
    private GUIStyle boxStyle;
    private GUIStyle toolButtonStyle;
    private GUIStyle toolButtonActiveStyle;
    private bool stylesInitialized = false;

    [MenuItem("Window/PikaGame/Map Editor")]
    public static void ShowWindow()
    {
        GetWindow<MapEditorWindow>("Map Editor");
    }

    private void InitializeStyles()
    {
        if (stylesInitialized) return;
        headerStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(5, 5, 5, 5) };
        boxStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(10, 10, 10, 10), margin = new RectOffset(5, 5, 5, 5) };
        toolButtonStyle = new GUIStyle(GUI.skin.button) { fixedHeight = 28, margin = new RectOffset(2, 2, 2, 2) };
        toolButtonActiveStyle = new GUIStyle(GUI.skin.button) { fixedHeight = 28, margin = new RectOffset(2, 2, 2, 2) };
        toolButtonActiveStyle.normal.textColor = Color.cyan;
        stylesInitialized = true;
    }

    public int Rows => rows;
    public int Columns => columns;
    public float TimeLimit => timeLimit;
    public List<BlockData> Blocks => blocks;
    public List<DoorSpawnData> Doors => doors;
    public List<WallSpawnData> Walls => walls;

    public void SetRows(int value) => rows = value;
    public void SetColumns(int value) => columns = value;
    public void SetTimeLimit(float value) => timeLimit = value;

    private void OnGUI()
    {
        InitializeStyles();
        HandleKeyboard();
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        GUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("PikaGame Level Editor", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Preview Level", GUILayout.Width(120), GUILayout.Height(26)))
            StartPreview();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);
        DrawToolbar();
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        if (showPreview) DrawInteractiveGrid();
        EditorGUILayout.BeginVertical(GUILayout.Width(220));

        DrawValidationPanel();
        EditorGUILayout.Space(5);
        DrawMapSettingsPanel();
        EditorGUILayout.Space(5);
        DrawPropertyInspector();
        EditorGUILayout.Space(5);
        DrawSelectionList();
        EditorGUILayout.Space(5);
        DrawSaveLoadPanel();

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
    }

    private void HandleKeyboard()
    {
        bool ctrl = Event.current.control || Event.current.command;
        Event e = Event.current;
        if (e.type != EventType.KeyDown) return;

        if (ctrl && e.keyCode == KeyCode.Z)
        {
            commandHistory.Undo();
            ClearSelection();
            e.Use();
        }
        else if (ctrl && e.keyCode == KeyCode.Y)
        {
            commandHistory.Redo();
            ClearSelection();
            e.Use();
        }
        else if (e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace)
        {
            if (hasSelection && GUIUtility.keyboardControl == 0)
            {
                DeleteSelected();
                e.Use();
            }
        }
        else if (e.keyCode == KeyCode.Escape)
        {
            ClearSelection();
            currentTool = Tool.Select;
            Repaint();
        }
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Select", currentTool == Tool.Select ? toolButtonActiveStyle : toolButtonStyle, GUILayout.Width(80)))
            currentTool = Tool.Select;
        if (GUILayout.Button("Block", currentTool == Tool.PlaceBlock ? toolButtonActiveStyle : toolButtonStyle, GUILayout.Width(80)))
            currentTool = Tool.PlaceBlock;
        if (GUILayout.Button("Door", currentTool == Tool.PlaceDoor ? toolButtonActiveStyle : toolButtonStyle, GUILayout.Width(80)))
            currentTool = Tool.PlaceDoor;
        if (GUILayout.Button("Wall", currentTool == Tool.PlaceWall ? toolButtonActiveStyle : toolButtonStyle, GUILayout.Width(80)))
            currentTool = Tool.PlaceWall;
        if (GUILayout.Button("Eraser", currentTool == Tool.Eraser ? toolButtonActiveStyle : toolButtonStyle, GUILayout.Width(80)))
            currentTool = Tool.Eraser;

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Fill Border", GUILayout.Width(100)))
            FillEmptyBorder();
        GUILayout.FlexibleSpace();

        GUI.enabled = commandHistory.CanUndo;
        if (GUILayout.Button("Undo", GUILayout.Width(70)))
        {
            commandHistory.Undo();
            ClearSelection();
        }
        GUI.enabled = true;

        GUI.enabled = commandHistory.CanRedo;
        if (GUILayout.Button("Redo", GUILayout.Width(70)))
        {
            commandHistory.Redo();
            ClearSelection();
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (currentTool == Tool.PlaceBlock)
        {
            GUILayout.Label("Type:", GUILayout.Width(35));
            int typeIdx = System.Array.IndexOf(blockTypes, selectedBlockType);
            typeIdx = EditorGUILayout.Popup(typeIdx, blockTypes, GUILayout.Width(100));
            selectedBlockType = blockTypes[typeIdx];

            GUILayout.Label("Color:", GUILayout.Width(40));
            int colorIdx = System.Array.IndexOf(colors, selectedColor);
            colorIdx = EditorGUILayout.Popup(colorIdx, colors, GUILayout.Width(80));
            selectedColor = colors[colorIdx];

            placeBlockHasMine = EditorGUILayout.Toggle("Mine", placeBlockHasMine, GUILayout.Width(55));
            if (placeBlockHasMine)
            {
                placeBlockMineCount = EditorGUILayout.IntField(placeBlockMineCount, GUILayout.Width(50));
                if (placeBlockMineCount < 1) placeBlockMineCount = 1;
            }

            placeBlockHasSecondColor = EditorGUILayout.Toggle("2nd C", placeBlockHasSecondColor, GUILayout.Width(55));
            if (placeBlockHasSecondColor)
            {
                int scIdx = System.Array.IndexOf(colors, placeBlockSecondColor);
                scIdx = EditorGUILayout.Popup(scIdx, colors, GUILayout.Width(65));
                placeBlockSecondColor = colors[scIdx];
            }
        }
        else if (currentTool == Tool.PlaceDoor)
        {
            GUILayout.Label("Dir:", GUILayout.Width(30));
            int dirIdx = System.Array.IndexOf(directions, doorDirection);
            dirIdx = EditorGUILayout.Popup(dirIdx, directions, GUILayout.Width(70));
            doorDirection = directions[dirIdx];

            GUILayout.Label("Type:", GUILayout.Width(35));
            int typeIdx = System.Array.IndexOf(doorTypes, doorType);
            typeIdx = EditorGUILayout.Popup(typeIdx, doorTypes, GUILayout.Width(70));
            doorType = doorTypes[typeIdx];

            GUILayout.Label("Color:", GUILayout.Width(40));
            int colorIdx = System.Array.IndexOf(colors, doorColor);
            colorIdx = EditorGUILayout.Popup(colorIdx, colors, GUILayout.Width(70));
            doorColor = colors[colorIdx];
        }
        else if (currentTool == Tool.PlaceWall)
        {
            GUILayout.Label("Dir:", GUILayout.Width(30));
            int dirIdx = System.Array.IndexOf(directions, wallDirection);
            dirIdx = EditorGUILayout.Popup(dirIdx, directions, GUILayout.Width(80));
            wallDirection = directions[dirIdx];
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawInteractiveGrid()
    {
        EditorGUILayout.LabelField("Map Grid", headerStyle);
        EditorGUILayout.BeginVertical(boxStyle);

        int previewRows = rows + 2;
        int previewColumns = columns + 2;
        int totalWidth = LevelEditorGrid.LabelSize + previewColumns * LevelEditorGrid.CellSize;
        int totalHeight = LevelEditorGrid.LabelSize + previewRows * LevelEditorGrid.CellSize;
        Rect previewArea = GUILayoutUtility.GetRect(totalWidth, totalHeight);
        EditorGUI.DrawRect(previewArea, new Color(0.12f, 0.12f, 0.12f));

        Rect gridOrigin = new Rect(previewArea.x + LevelEditorGrid.LabelSize, previewArea.y + LevelEditorGrid.LabelSize, 0, 0);
        LevelEditorGrid.DrawGridBackground(gridOrigin, rows, columns);
        HandleGridMouse(gridOrigin);
        DrawGhostPreview(gridOrigin);
        DrawSelectionHighlight(gridOrigin);

        foreach (var block in blocks)
            LevelEditorGrid.DrawBlockFootprint(gridOrigin, block, rows, columns);
        foreach (var door in doors)
            LevelEditorGrid.DrawDoor(gridOrigin, door, rows, columns);
        foreach (var wall in walls)
            LevelEditorGrid.DrawWall(gridOrigin, wall, rows, columns);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField($"Map: {rows}x{columns} | Blocks: {blocks.Count} | Doors: {doors.Count} | Walls: {walls.Count}", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.EndVertical();
    }

    private void HandleGridMouse(Rect gridOrigin)
    {
        Event e = Event.current;
        if (e.type == EventType.Repaint) return;

        Rect gridArea = new Rect(gridOrigin.x - LevelEditorGrid.LabelSize, gridOrigin.y - LevelEditorGrid.LabelSize,
            (columns + 2) * LevelEditorGrid.CellSize, (rows + 2) * LevelEditorGrid.CellSize);
        if (!gridArea.Contains(e.mousePosition)) return;

        Vector2Int visualCell = LevelEditorGrid.ScreenToGrid(e.mousePosition, gridOrigin);
        int mapRow = visualCell.y - 1;
        int mapCol = visualCell.x - 1;
        bool inPlayable = LevelEditorGrid.IsInBounds(mapRow, mapCol, rows, columns);
        bool onAnyBorder = !inPlayable && LevelEditorGrid.IsDoorBorderCell(mapRow, mapCol, rows, columns);
        bool inBounds = inPlayable || onAnyBorder;

        if (inBounds) hoverCell = new Vector2Int(mapCol, mapRow);
        else hoverCell = null;

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            isMouseDown = true;
            dragStartCell = inBounds ? new Vector2Int(mapCol, mapRow) : (Vector2Int?)null;
            e.Use();
        }
        if (e.type == EventType.MouseDrag && isMouseDown && e.button == 0)
        {
            if (inBounds && (dragStartCell == null || dragStartCell.Value != new Vector2Int(mapCol, mapRow)))
                Repaint();
            e.Use();
        }
        if (e.type == EventType.MouseUp && e.button == 0 && isMouseDown)
        {
            isMouseDown = false;
            if (inBounds && dragStartCell != null)
                HandleGridClick(mapRow, mapCol);
            dragStartCell = null;
            e.Use();
        }
        if (e.type == EventType.MouseMove) Repaint();
    }

    private void HandleGridClick(int row, int col)
    {
        switch (currentTool)
        {
            case Tool.Select: SelectObjectAt(row, col); break;
            case Tool.PlaceBlock: PlaceBlockAt(row, col); break;
            case Tool.PlaceDoor: PlaceDoorAt(row, col); break;
            case Tool.PlaceWall: PlaceWallAt(row, col); break;
            case Tool.Eraser: EraseAt(row, col); break;
        }
        Repaint();
    }

    private void SelectObjectAt(int row, int col)
    {
        ClearSelection();
        for (int i = 0; i < blocks.Count; i++)
        {
            var footprint = LevelEditorGrid.GetBlockFootprint(blocks[i].blockType);
            foreach (var offset in footprint)
            {
                if (blocks[i].row + offset.y == row && blocks[i].column + offset.x == col)
                {
                    selectedBlockIndex = i;
                    return;
                }
            }
        }
        for (int i = 0; i < doors.Count; i++)
        {
            var foot = LevelEditorGrid.GetDoorFootprint(doors[i].doorType, doors[i].direction);
            foreach (var offset in foot)
            {
                if (doors[i].row + offset.y == row && doors[i].column + offset.x == col)
                {
                    selectedDoorIndex = i;
                    return;
                }
            }
        }
        for (int i = 0; i < walls.Count; i++)
        {
            if (walls[i].row == row && walls[i].column == col)
            {
                selectedWallIndex = i;
                return;
            }
        }
    }

    private void PlaceBlockAt(int row, int col)
    {
        BlockData newBlock = new BlockData
        {
            blockType = selectedBlockType,
            row = row,
            column = col,
            color = selectedColor,
            hasMine = placeBlockHasMine,
            mineCount = placeBlockHasMine ? placeBlockMineCount : 0,
            hasSecondColor = placeBlockHasSecondColor,
            secondColor = placeBlockHasSecondColor ? placeBlockSecondColor : null
        };
        if (LevelEditorGrid.HasBlockOverlap(newBlock, blocks, doors, walls, rows, columns))
        {
            EditorUtility.DisplayDialog("Cannot Place", "This position overlaps with an existing object or is out of bounds.", "OK");
            return;
        }
        var cmd = new PlaceObjectCommand<BlockData>(blocks, newBlock, blocks.Count);
        commandHistory.Execute(cmd);
        ClearSelection();
        selectedBlockIndex = blocks.Count - 1;
    }

    private void PlaceDoorAt(int row, int col)
    {
        DoorSpawnData newDoor = new DoorSpawnData
        {
            row = row,
            column = col,
            direction = doorDirection,
            doorType = doorType,
            color = doorColor
        };
        if (LevelEditorGrid.HasDoorOverlap(newDoor, blocks, doors, walls, rows, columns))
        {
            EditorUtility.DisplayDialog("Cannot Place", "This position overlaps or is out of bounds.", "OK");
            return;
        }
        var cmd = new PlaceObjectCommand<DoorSpawnData>(doors, newDoor, doors.Count);
        commandHistory.Execute(cmd);
        ClearSelection();
        selectedDoorIndex = doors.Count - 1;
    }

    private void PlaceWallAt(int row, int col)
    {
        WallSpawnData newWall = new WallSpawnData
        {
            row = row,
            column = col,
            direction = wallDirection
        };
        if (LevelEditorGrid.HasWallOverlap(newWall, blocks, doors, walls, rows, columns))
        {
            EditorUtility.DisplayDialog("Cannot Place", "This position overlaps or is out of bounds.", "OK");
            return;
        }
        var cmd = new PlaceObjectCommand<WallSpawnData>(walls, newWall, walls.Count);
        commandHistory.Execute(cmd);
        ClearSelection();
        selectedWallIndex = walls.Count - 1;
    }

    private void EraseAt(int row, int col)
    {
        for (int i = blocks.Count - 1; i >= 0; i--)
        {
            var footprint = LevelEditorGrid.GetBlockFootprint(blocks[i].blockType);
            foreach (var offset in footprint)
            {
                if (blocks[i].row + offset.y == row && blocks[i].column + offset.x == col)
                {
                    var cmd = new RemoveObjectCommand<BlockData>(blocks, blocks[i], i);
                    commandHistory.Execute(cmd);
                    if (selectedBlockIndex == i) ClearSelection();
                    else if (selectedBlockIndex > i) selectedBlockIndex--;
                    return;
                }
            }
        }
        for (int i = doors.Count - 1; i >= 0; i--)
        {
            var foot = LevelEditorGrid.GetDoorFootprint(doors[i].doorType, doors[i].direction);
            foreach (var offset in foot)
            {
                if (doors[i].row + offset.y == row && doors[i].column + offset.x == col)
                {
                    var cmd = new RemoveObjectCommand<DoorSpawnData>(doors, doors[i], i);
                    commandHistory.Execute(cmd);
                    if (selectedDoorIndex == i) ClearSelection();
                    else if (selectedDoorIndex > i) selectedDoorIndex--;
                    return;
                }
            }
        }
        for (int i = walls.Count - 1; i >= 0; i--)
        {
            if (walls[i].row == row && walls[i].column == col)
            {
                var cmd = new RemoveObjectCommand<WallSpawnData>(walls, walls[i], i);
                commandHistory.Execute(cmd);
                if (selectedWallIndex == i) ClearSelection();
                else if (selectedWallIndex > i) selectedWallIndex--;
                return;
            }
        }
    }

    private void DeleteSelected()
    {
        if (selectedBlockIndex >= 0 && selectedBlockIndex < blocks.Count)
        {
            var cmd = new RemoveObjectCommand<BlockData>(blocks, blocks[selectedBlockIndex], selectedBlockIndex);
            commandHistory.Execute(cmd);
            ClearSelection();
        }
        else if (selectedDoorIndex >= 0 && selectedDoorIndex < doors.Count)
        {
            var cmd = new RemoveObjectCommand<DoorSpawnData>(doors, doors[selectedDoorIndex], selectedDoorIndex);
            commandHistory.Execute(cmd);
            ClearSelection();
        }
        else if (selectedWallIndex >= 0 && selectedWallIndex < walls.Count)
        {
            var cmd = new RemoveObjectCommand<WallSpawnData>(walls, walls[selectedWallIndex], selectedWallIndex);
            commandHistory.Execute(cmd);
            ClearSelection();
        }
    }

    private void ClearSelection()
    {
        selectedBlockIndex = -1;
        selectedDoorIndex = -1;
        selectedWallIndex = -1;
    }

    private void DrawGhostPreview(Rect gridOrigin)
    {
        if (hoverCell == null) return;
        if (isMouseDown) return;

        int row = hoverCell.Value.y;
        int col = hoverCell.Value.x;

        switch (currentTool)
        {
            case Tool.PlaceBlock:
                var ghostBlock = new BlockData
                {
                    blockType = selectedBlockType,
                    row = row,
                    column = col,
                    color = selectedColor,
                    hasMine = placeBlockHasMine,
                    mineCount = placeBlockHasMine ? placeBlockMineCount : 0,
                    hasSecondColor = placeBlockHasSecondColor,
                    secondColor = placeBlockHasSecondColor ? placeBlockSecondColor : null
                };
                bool blockValid = !LevelEditorGrid.HasBlockOverlap(ghostBlock, blocks, doors, walls, rows, columns);
                LevelEditorGrid.DrawGhostFootprint(gridOrigin, ghostBlock, blockValid, rows, columns);
                break;
            case Tool.PlaceDoor:
                var ghostDoor = new DoorSpawnData
                {
                    row = row,
                    column = col,
                    direction = doorDirection,
                    doorType = doorType,
                    color = doorColor
                };
                bool doorValid = !LevelEditorGrid.HasDoorOverlap(ghostDoor, blocks, doors, walls, rows, columns);
                LevelEditorGrid.DrawGhostDoor(gridOrigin, ghostDoor, doorValid, rows, columns);
                break;
            case Tool.PlaceWall:
                var ghostWall = new WallSpawnData
                {
                    row = row,
                    column = col,
                    direction = wallDirection
                };
                bool wallValid = !LevelEditorGrid.HasWallOverlap(ghostWall, blocks, doors, walls, rows, columns);
                LevelEditorGrid.DrawGhostWall(gridOrigin, ghostWall, wallValid, rows, columns);
                break;
            case Tool.Eraser:
                if (LevelEditorGrid.IsInBounds(row, col, rows, columns))
                {
                    int vr = row + 1;
                    int vc = col + 1;
                    Rect cellRect = new Rect(gridOrigin.x + vc * LevelEditorGrid.CellSize, gridOrigin.y + vr * LevelEditorGrid.CellSize, LevelEditorGrid.CellSize, LevelEditorGrid.CellSize);
                    EditorGUI.DrawRect(cellRect, new Color(1f, 0.2f, 0.2f, 0.3f));
                    LevelEditorGrid.DrawCellBorder(cellRect, Color.red);
                }
                break;
        }
    }

    private void DrawSelectionHighlight(Rect gridOrigin)
    {
        if (!hasSelection) return;

        if (selectedBlockIndex >= 0 && selectedBlockIndex < blocks.Count)
        {
            var footprint = LevelEditorGrid.GetBlockFootprint(blocks[selectedBlockIndex].blockType);
            var offsets = new List<Vector2Int>();
            foreach (var offset in footprint)
                offsets.Add(new Vector2Int(blocks[selectedBlockIndex].column + offset.x, blocks[selectedBlockIndex].row + offset.y));
            LevelEditorGrid.DrawSelectionHighlight(gridOrigin, offsets, rows, columns);
        }
        else if (selectedDoorIndex >= 0 && selectedDoorIndex < doors.Count)
        {
            var d = doors[selectedDoorIndex];
            var offsets = new List<Vector2Int> { new Vector2Int(d.column, d.row) };
            LevelEditorGrid.DrawSelectionHighlight(gridOrigin, offsets, rows, columns);
        }
        else if (selectedWallIndex >= 0 && selectedWallIndex < walls.Count)
        {
            var w = walls[selectedWallIndex];
            var offsets = new List<Vector2Int> { new Vector2Int(w.column, w.row) };
            LevelEditorGrid.DrawSelectionHighlight(gridOrigin, offsets, rows, columns);
        }
    }

    private void DrawValidationPanel()
    {
        EditorGUILayout.LabelField("Validation", headerStyle);
        EditorGUILayout.BeginVertical(boxStyle);
        LevelData levelData = new LevelData
        {
            rows = rows,
            columns = columns,
            blocks = blocks,
            doors = doors,
            walls = walls
        };
        var errors = LevelEditorGrid.GetAllValidationErrors(levelData);
        if (errors.Count == 0)
            EditorGUILayout.HelpBox("No issues found.", MessageType.Info);
        else
            EditorGUILayout.HelpBox(errors.Count + " issue(s):\n" + string.Join("\n", errors), MessageType.Warning);
        EditorGUILayout.EndVertical();
    }

    private void DrawMapSettingsPanel()
    {
        EditorGUILayout.LabelField("Map Settings", headerStyle);
        EditorGUILayout.BeginVertical(boxStyle);
        EditorGUI.BeginChangeCheck();
        int newRows = EditorGUILayout.IntSlider("Rows", rows, 0, 30);
        int newColumns = EditorGUILayout.IntSlider("Columns", columns, 0, 30);
        float newTimeLimit = EditorGUILayout.FloatField("Time Limit", timeLimit);
        if (EditorGUI.EndChangeCheck() && (newRows != rows || newColumns != columns || newTimeLimit != timeLimit))
        {
            var cmd = new ResizeMapCommand(this, newRows, newColumns, newTimeLimit);
            commandHistory.Execute(cmd);
            ClearSelection();
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawPropertyInspector()
    {
        EditorGUILayout.LabelField("Property Inspector", headerStyle);
        EditorGUILayout.BeginVertical(boxStyle);

        if (selectedBlockIndex >= 0 && selectedBlockIndex < blocks.Count)
        {
            BlockData block = blocks[selectedBlockIndex];
            EditorGUI.BeginChangeCheck();

            int typeIdx = System.Array.IndexOf(blockTypes, block.blockType);
            typeIdx = EditorGUILayout.Popup("Type", typeIdx, blockTypes);
            int colorIdx = System.Array.IndexOf(colors, block.color);
            colorIdx = EditorGUILayout.Popup("Color", colorIdx, colors);
            int row = EditorGUILayout.IntField("Row", block.row);
            int col = EditorGUILayout.IntField("Column", block.column);

            bool hasMine = EditorGUILayout.Toggle("Has Mine", block.hasMine);
            int mineCount = block.hasMine ? EditorGUILayout.IntField("Mine Count", block.mineCount) : block.mineCount;
            if (mineCount < 1 && block.hasMine) mineCount = 1;

            bool hasSecondColor = EditorGUILayout.Toggle("2nd Color", block.hasSecondColor);
            string secondColor = block.secondColor ?? "Red";
            if (hasSecondColor)
            {
                int scIdx = System.Array.IndexOf(colors, secondColor);
                if (scIdx < 0) scIdx = 0;
                scIdx = EditorGUILayout.Popup("", scIdx, colors);
                secondColor = colors[scIdx];
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (typeIdx >= 0) ApplyProperty(block, "blockType", blockTypes[typeIdx]);
                if (colorIdx >= 0) ApplyProperty(block, "color", colors[colorIdx]);
                if (row != block.row) ApplyProperty(block, "row", row);
                if (col != block.column) ApplyProperty(block, "column", col);
                if (hasMine != block.hasMine) ApplyProperty(block, "hasMine", hasMine);
                if (mineCount != block.mineCount) ApplyProperty(block, "mineCount", mineCount);
                if (hasSecondColor != block.hasSecondColor) ApplyProperty(block, "hasSecondColor", hasSecondColor);
                if (block.hasSecondColor && secondColor != block.secondColor) ApplyProperty(block, "secondColor", secondColor);
            }

            if (GUILayout.Button("Delete Block", GUILayout.Height(24)))
                DeleteSelected();
        }
        else if (selectedDoorIndex >= 0 && selectedDoorIndex < doors.Count)
        {
            DoorSpawnData door = doors[selectedDoorIndex];
            EditorGUI.BeginChangeCheck();
            int dirIdx = System.Array.IndexOf(directions, door.direction);
            dirIdx = EditorGUILayout.Popup("Direction", dirIdx, directions);
            int typeIdx = System.Array.IndexOf(doorTypes, door.doorType);
            typeIdx = EditorGUILayout.Popup("Type", typeIdx, doorTypes);
            int colorIdx = System.Array.IndexOf(colors, door.color);
            colorIdx = EditorGUILayout.Popup("Color", colorIdx, colors);
            int row = EditorGUILayout.IntField("Row", door.row);
            int col = EditorGUILayout.IntField("Column", door.column);
            if (EditorGUI.EndChangeCheck())
            {
                if (dirIdx >= 0) ApplyProperty(door, "direction", directions[dirIdx]);
                if (typeIdx >= 0) ApplyProperty(door, "doorType", doorTypes[typeIdx]);
                if (colorIdx >= 0) ApplyProperty(door, "color", colors[colorIdx]);
                if (row != door.row) ApplyProperty(door, "row", row);
                if (col != door.column) ApplyProperty(door, "column", col);
            }
            if (GUILayout.Button("Delete Door", GUILayout.Height(24)))
                DeleteSelected();
        }
        else if (selectedWallIndex >= 0 && selectedWallIndex < walls.Count)
        {
            WallSpawnData wall = walls[selectedWallIndex];
            EditorGUI.BeginChangeCheck();
            int dirIdx = System.Array.IndexOf(directions, wall.direction);
            dirIdx = EditorGUILayout.Popup("Direction", dirIdx, directions);
            int row = EditorGUILayout.IntField("Row", wall.row);
            int col = EditorGUILayout.IntField("Column", wall.column);
            if (EditorGUI.EndChangeCheck())
            {
                if (dirIdx >= 0) ApplyProperty(wall, "direction", directions[dirIdx]);
                if (row != wall.row) ApplyProperty(wall, "row", row);
                if (col != wall.column) ApplyProperty(wall, "column", col);
            }
            if (GUILayout.Button("Delete Wall", GUILayout.Height(24)))
                DeleteSelected();
        }
        else
        {
            EditorGUILayout.HelpBox("Select an object on the grid to edit its properties.", MessageType.Info);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawSelectionList()
    {
        EditorGUILayout.LabelField("Objects", headerStyle);
        EditorGUILayout.BeginVertical(boxStyle);

        EditorGUILayout.LabelField("Blocks (" + blocks.Count + ")", EditorStyles.miniBoldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Blocks", GUILayout.Height(20)))
        {
            if (EditorUtility.DisplayDialog("Confirm", "Clear all blocks?", "Yes", "No"))
            {
                blocks.Clear();
                ClearSelection();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Doors (" + doors.Count + ")", EditorStyles.miniBoldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Doors", GUILayout.Height(20)))
        {
            if (EditorUtility.DisplayDialog("Confirm", "Clear all doors?", "Yes", "No"))
            {
                doors.Clear();
                ClearSelection();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Walls", GUILayout.Height(20)))
        {
            if (EditorUtility.DisplayDialog("Confirm", "Clear all walls?", "Yes", "No"))
            {
                walls.Clear();
                ClearSelection();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawSaveLoadPanel()
    {
        EditorGUILayout.LabelField("Save / Load", headerStyle);
        EditorGUILayout.BeginVertical(boxStyle);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Name:", GUILayout.Width(40));
        levelName = EditorGUILayout.TextField(levelName, GUILayout.Width(120));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save", GUILayout.Height(28)))
            SaveMap();
        if (GUILayout.Button("Load", GUILayout.Height(28)))
            LoadMap();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5);
        if (GUILayout.Button("New Map", GUILayout.Height(28)))
            NewMap();
        EditorGUILayout.EndVertical();
    }

    private void ApplyProperty(object target, string fieldName, object newValue)
    {
        var cmd = new EditPropertyCommand(target, fieldName, newValue);
        commandHistory.Execute(cmd);
        Repaint();
    }

    private void SaveMap()
    {
        if (string.IsNullOrEmpty(levelName))
        {
            EditorUtility.DisplayDialog("Error", "Please enter a level name.", "OK");
            return;
        }
        LevelData levelData = new LevelData
        {
            rows = rows,
            columns = columns,
            timeLimit = timeLimit,
            blocks = new List<BlockData>(blocks),
            doors = ConvertListForSave(doors),
            walls = ConvertListForSave(walls)
        };
        LevelDataWrapper wrapper = new LevelDataWrapper { level = levelData };
        string json = JsonUtility.ToJson(wrapper, true);
        string levelsPath = Path.Combine(Application.streamingAssetsPath, "Levels");
        if (!Directory.Exists(levelsPath)) Directory.CreateDirectory(levelsPath);
        string filePath = Path.Combine(levelsPath, levelName + ".json");
        File.WriteAllText(filePath, json);
        commandHistory.Clear();
        EditorUtility.DisplayDialog("Success", "Map saved as " + levelName + ".json", "OK");
    }

    private void LoadMap()
    {
        if (string.IsNullOrEmpty(levelName))
        {
            EditorUtility.DisplayDialog("Error", "Please enter a level name.", "OK");
            return;
        }
        string levelsPath = Path.Combine(Application.streamingAssetsPath, "Levels");
        string filePath = Path.Combine(levelsPath, levelName + ".json");
        if (!File.Exists(filePath))
        {
            EditorUtility.DisplayDialog("Error", "File " + levelName + ".json not found in Levels folder.", "OK");
            return;
        }
        string json = File.ReadAllText(filePath);
        LevelDataWrapper wrapper = JsonUtility.FromJson<LevelDataWrapper>(json);
        LevelData levelData = wrapper.level;
        if (levelData == null)
        {
            EditorUtility.DisplayDialog("Error", "Invalid level data in file.", "OK");
            return;
        }
        rows = levelData.rows;
        columns = levelData.columns;
        timeLimit = levelData.timeLimit;
        blocks = new List<BlockData>(levelData.blocks);
        doors = ConvertListForLoad(levelData.doors);
        walls = ConvertListForLoad(levelData.walls);
        commandHistory.Clear();
        ClearSelection();
        Repaint();
        EditorUtility.DisplayDialog("Success", "Map " + levelName + ".json loaded.", "OK");
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
            commandHistory.Clear();
            ClearSelection();
            Repaint();
        }
    }

    private void StartPreview()
    {
        LevelEditorPreview.StartPreview(this);
    }

    private static List<DoorSpawnData> ConvertListForSave(List<DoorSpawnData> doors)
    {
        var result = new List<DoorSpawnData>(doors.Count);
        foreach (var door in doors) result.Add(DoorWallCoordinateConverter.ConvertForSave(door));
        return result;
    }

    private static List<WallSpawnData> ConvertListForSave(List<WallSpawnData> walls)
    {
        var result = new List<WallSpawnData>(walls.Count);
        foreach (var wall in walls) result.Add(DoorWallCoordinateConverter.ConvertForSave(wall));
        return result;
    }

    private static List<DoorSpawnData> ConvertListForLoad(List<DoorSpawnData> doors)
    {
        var result = new List<DoorSpawnData>(doors.Count);
        foreach (var door in doors) result.Add(DoorWallCoordinateConverter.ConvertForLoad(door));
        return result;
    }

    private static List<WallSpawnData> ConvertListForLoad(List<WallSpawnData> walls)
    {
        var result = new List<WallSpawnData>(walls.Count);
        foreach (var wall in walls) result.Add(DoorWallCoordinateConverter.ConvertForLoad(wall));
        return result;
    }

    private HashSet<Vector2Int> GetDoorOccupiedBorderCells()
    {
        var occupied = new HashSet<Vector2Int>();
        foreach (var door in doors)
        {
            var footprint = LevelEditorGrid.GetDoorFootprint(door.doorType, door.direction);
            foreach (var offset in footprint)
            {
                int r = door.row + offset.y;
                int c = door.column + offset.x;
                if (LevelEditorGrid.IsDoorBorderCell(r, c, rows, columns))
                    occupied.Add(new Vector2Int(c, r));
            }
        }
        return occupied;
    }

    private bool IsBorderCell(int row, int col)
    {
        bool onTop = row == -1 && col >= 0 && col < columns;
        bool onBottom = row == rows && col >= 0 && col < columns;
        bool onLeft = col == -1 && row >= 0 && row < rows;
        bool onRight = col == columns && row >= 0 && row < rows;
        return onTop || onBottom || onLeft || onRight;
    }

    private void FillEmptyBorder()
    {
        var doorOccupied = GetDoorOccupiedBorderCells();
        int added = 0;
        var topWalls = new List<Vector2Int>();
        var bottomWalls = new List<Vector2Int>();
        var leftWalls = new List<Vector2Int>();
        var rightWalls = new List<Vector2Int>();

        for (int c = 0; c < columns; c++)
        {
            if (!doorOccupied.Contains(new Vector2Int(c, -1))) topWalls.Add(new Vector2Int(c, -1));
            if (!doorOccupied.Contains(new Vector2Int(c, rows))) bottomWalls.Add(new Vector2Int(c, rows));
        }
        for (int r = 0; r < rows; r++)
        {
            if (!doorOccupied.Contains(new Vector2Int(-1, r))) leftWalls.Add(new Vector2Int(-1, r));
            if (!doorOccupied.Contains(new Vector2Int(columns, r))) rightWalls.Add(new Vector2Int(columns, r));
        }

        var allNewWalls = new List<WallSpawnData>();
        foreach (var p in topWalls) allNewWalls.Add(new WallSpawnData { row = p.y, column = p.x, direction = "Up" });
        foreach (var p in bottomWalls) allNewWalls.Add(new WallSpawnData { row = p.y, column = p.x, direction = "Down" });
        foreach (var p in leftWalls) allNewWalls.Add(new WallSpawnData { row = p.y, column = p.x, direction = "Left" });
        foreach (var p in rightWalls) allNewWalls.Add(new WallSpawnData { row = p.y, column = p.x, direction = "Right" });

        var existingWallSet = new HashSet<Vector2Int>();
        foreach (var w in walls) existingWallSet.Add(new Vector2Int(w.column, w.row));

        foreach (var newWall in allNewWalls)
        {
            var key = new Vector2Int(newWall.column, newWall.row);
            if (!existingWallSet.Contains(key))
            {
                var cmd = new PlaceObjectCommand<WallSpawnData>(walls, newWall, walls.Count);
                commandHistory.Execute(cmd);
                existingWallSet.Add(key);
                added++;
            }
        }

        if (added > 0) Debug.Log($"Fill Border: added {added} walls.");
        else Debug.Log("Fill Border: border already fully walled.");
        Repaint();
    }
}
