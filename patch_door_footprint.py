import re

path = r"d:\GameUnity\PikaGame29\Assets\Scripts\Editor\LevelEditorGrid.cs"
with open(path, "r", encoding="utf-8") as f:
    content = f.read()

# --- Fix 1: DrawDoor - render full door footprint ---
old_draw_door = (
    'public static void DrawDoor(Rect gridOrigin, DoorSpawnData door, int rows, int columns)\n'
    '    {\n'
    '        int mapRow = door.row;\n'
    '        int mapColumn = door.column;\n'
    '        if (!IsDoorInBounds(mapRow, mapColumn, rows, columns)) return;\n'
    '        int visualRow = mapRow + 1;\n'
    '        int visualCol = mapColumn + 1;\n'
    '        Rect doorRect = new Rect(\n'
    '            gridOrigin.x + visualCol * CellSize,\n'
    '            gridOrigin.y + visualRow * CellSize,\n'
    '            CellSize, CellSize);\n'
    '        Color baseColor = GetEditorColor(door.color);\n'
    '        Color fillColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.70f);\n'
    '        EditorGUI.DrawRect(doorRect, fillColor);\n'
    '        DrawCellBorder(doorRect, Color.white);\n'
    '        Rect innerRect = new Rect(doorRect.x + 3, doorRect.y + 3, doorRect.width - 6, doorRect.height - 6);\n'
    '        EditorGUI.DrawRect(innerRect, new Color(0f, 0f, 0f, 0.18f));\n'
    '        string label = GetDoorShortLabel(door);\n'
    '        GUI.Label(doorRect, label, EditorStyles.centeredGreyMiniLabel);\n'
    '        string dir = door.direction switch { "Up" => "^", "Down" => "v", "Left" => "<", "Right" => ">", _ => "?" };\n'
    '        Rect dirRect = new Rect(doorRect.x, doorRect.y + doorRect.height * 0.5f, doorRect.width, doorRect.height * 0.5f);\n'
    '        GUI.Label(dirRect, dir, EditorStyles.miniLabel);\n'
    '    }'
)

new_draw_door = (
    'public static void DrawDoor(Rect gridOrigin, DoorSpawnData door, int rows, int columns)\n'
    '    {\n'
    '        var footprint = GetDoorFootprint(door.doorType, door.direction);\n'
    '        Color baseColor = GetEditorColor(door.color);\n'
    '        Color fillColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.70f);\n'
    '        for (int i = 0; i < footprint.Count; i++)\n'
    '        {\n'
    '            Vector2Int offset = footprint[i];\n'
    '            int mapRow = door.row + offset.y;\n'
    '            int mapColumn = door.column + offset.x;\n'
    '            if (!IsDoorInBounds(mapRow, mapColumn, rows, columns)) continue;\n'
    '            int visualRow = mapRow + 1;\n'
    '            int visualCol = mapColumn + 1;\n'
    '            Rect doorRect = new Rect(\n'
    '                gridOrigin.x + visualCol * CellSize,\n'
    '                gridOrigin.y + visualRow * CellSize,\n'
    '                CellSize, CellSize);\n'
    '            EditorGUI.DrawRect(doorRect, fillColor);\n'
    '            DrawCellBorder(doorRect, Color.white);\n'
    '            Rect innerRect = new Rect(doorRect.x + 3, doorRect.y + 3, doorRect.width - 6, doorRect.height - 6);\n'
    '            EditorGUI.DrawRect(innerRect, new Color(0f, 0f, 0f, 0.18f));\n'
    '            if (i == 0)\n'
    '            {\n'
    '                string label = GetDoorShortLabel(door);\n'
    '                GUI.Label(doorRect, label, EditorStyles.centeredGreyMiniLabel);\n'
    '                string dir = door.direction switch { "Up" => "^", "Down" => "v", "Left" => "<", "Right" => ">", _ => "?" };\n'
    '                Rect dirRect = new Rect(doorRect.x, doorRect.y + doorRect.height * 0.5f, doorRect.width, doorRect.height * 0.5f);\n'
    '                GUI.Label(dirRect, dir, EditorStyles.miniLabel);\n'
    '            }\n'
    '        }\n'
    '    }'
)

if old_draw_door in content:
    content = content.replace(old_draw_door, new_draw_door, 1)
    print("Fix DrawDoor: OK")
else:
    print("Fix DrawDoor: NOT FOUND - searching...")
    idx = content.find("public static void DrawDoor")
    if idx >= 0:
        print(f"Found at index {idx}")
        print(repr(content[idx:idx+50]))

# --- Fix 2: DrawGhostDoor - render full door footprint ---
old_ghost_door = (
    'public static void DrawGhostDoor(Rect gridOrigin, DoorSpawnData ghostDoor, bool isValid, int rows, int columns)\n'
    '    {\n'
    '        int r = ghostDoor.row;\n'
    '        int c = ghostDoor.column;\n'
    '        bool inPlayable = IsInBounds(r, c, rows, columns);\n'
    '        bool onBorder = IsDoorBorderCell(r, c, rows, columns);\n'
    '        if (!inPlayable && !onBorder) return;\n'
    '        int visualRow = r + 1;\n'
    '        int visualCol = c + 1;\n'
    '        Rect cellRect = new Rect(\n'
    '            gridOrigin.x + visualCol * CellSize,\n'
    '            gridOrigin.y + visualRow * CellSize,\n'
    '            CellSize, CellSize);\n'
    '        Color fill = isValid ? new Color(0.2f, 0.8f, 1f, 0.35f) : new Color(1f, 0.2f, 0.2f, 0.35f);\n'
    '        Color border = isValid ? Color.cyan : Color.red;\n'
    '        EditorGUI.DrawRect(cellRect, fill);\n'
    '        DrawCellBorder(cellRect, border);\n'
    '    }'
)

new_ghost_door = (
    'public static void DrawGhostDoor(Rect gridOrigin, DoorSpawnData ghostDoor, bool isValid, int rows, int columns)\n'
    '    {\n'
    '        var footprint = GetDoorFootprint(ghostDoor.doorType, ghostDoor.direction);\n'
    '        bool allValid = isValid;\n'
    '        foreach (var offset in footprint)\n'
    '        {\n'
    '            int r = ghostDoor.row + offset.y;\n'
    '            int c = ghostDoor.column + offset.x;\n'
    '            bool inPlayable = IsInBounds(r, c, rows, columns);\n'
    '            bool onBorder = IsDoorBorderCell(r, c, rows, columns);\n'
    '            if (!inPlayable && !onBorder) allValid = false;\n'
    '        }\n'
    '        Color fill = allValid ? new Color(0.2f, 0.8f, 1f, 0.35f) : new Color(1f, 0.2f, 0.2f, 0.35f);\n'
    '        Color border = allValid ? Color.cyan : Color.red;\n'
    '        for (int i = 0; i < footprint.Count; i++)\n'
    '        {\n'
    '            Vector2Int offset = footprint[i];\n'
    '            int r = ghostDoor.row + offset.y;\n'
    '            int c = ghostDoor.column + offset.x;\n'
    '            int visualRow = r + 1;\n'
    '            int visualCol = c + 1;\n'
    '            Rect cellRect = new Rect(\n'
    '                gridOrigin.x + visualCol * CellSize,\n'
    '                gridOrigin.y + visualRow * CellSize,\n'
    '                CellSize, CellSize);\n'
    '            bool cellInPlayable = IsInBounds(r, c, rows, columns);\n'
    '            bool cellOnBorder = IsDoorBorderCell(r, c, rows, columns);\n'
    '            if (!cellInPlayable && !cellOnBorder)\n'
    '            {\n'
    '                EditorGUI.DrawRect(cellRect, new Color(1f, 0.2f, 0.2f, 0.35f));\n'
    '                DrawCellBorder(cellRect, Color.red);\n'
    '            }\n'
    '            else\n'
    '            {\n'
    '                EditorGUI.DrawRect(cellRect, fill);\n'
    '                DrawCellBorder(cellRect, border);\n'
    '            }\n'
    '        }\n'
    '    }'
)

if old_ghost_door in content:
    content = content.replace(old_ghost_door, new_ghost_door, 1)
    print("Fix DrawGhostDoor: OK")
else:
    print("Fix DrawGhostDoor: NOT FOUND")

# --- Fix 3: GetAllValidationErrors door overlap check ---
old_val_door = (
    'foreach (var wall in level.walls)\n'
    '        {\n'
    '            if (door.row == wall.row && door.column == wall.column)\n'
    '            {\n'
    '                errors.Add($"Door {door.doorType} at ({door.row},{door.column}) overlaps with Wall");\n'
    '                break;\n'
    '            }\n'
    '        }'
)

new_val_door = (
    'var doorFootprint = GetDoorFootprint(door.doorType, door.direction);\n'
    '            foreach (var wall in level.walls)\n'
    '            {\n'
    '                bool wallHit = false;\n'
    '                foreach (var dOff in doorFootprint)\n'
    '                {\n'
    '                    if (door.row + dOff.y == wall.row && door.column + dOff.x == wall.column)\n'
    '                    {\n'
    '                        wallHit = true;\n'
    '                        break;\n'
    '                    }\n'
    '                }\n'
    '                if (wallHit)\n'
    '                {\n'
    '                    errors.Add($"Door {door.doorType} at ({door.row},{door.column}) overlaps with Wall");\n'
    '                    break;\n'
    '                }\n'
    '            }'
)

if old_val_door in content:
    content = content.replace(old_val_door, new_val_door, 1)
    print("Fix validation door-wall overlap: OK")
else:
    print("Fix validation door-wall: NOT FOUND")

with open(path, "w", encoding="utf-8") as f:
    f.write(content)
print("All patches written!")
