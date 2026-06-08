import re

path = r"d:\GameUnity\PikaGame29\Assets\Scripts\Editor\MapEditorWindow.cs"
with open(path, "r", encoding="utf-8") as f:
    content = f.read()

# Fix 1: HandleGridMouse - allow door placement on border cells
old1 = """bool inBounds = LevelEditorGrid.IsInBounds(cell.y, cell.x, rows, columns);

// Update hover cell
if (inBounds)
{
    hoverCell = cell;
}
else
{
    hoverCell = null;
}"""

new1 = """bool inPlayable = LevelEditorGrid.IsInBounds(cell.y, cell.x, rows, columns);
bool onDoorBorder = !inPlayable && LevelEditorGrid.IsDoorBorderCell(cell.y, cell.x, rows, columns);
bool inBounds = inPlayable || (currentTool == Tool.PlaceDoor && onDoorBorder);

// Update hover cell
if (inBounds)
{
    hoverCell = cell;
}
else
{
    hoverCell = null;
}"""

if old1 in content:
    content = content.replace(old1, new1)
    print("Fix 1 applied: HandleGridMouse door border support")
else:
    print("ERROR: Fix 1 pattern not found!")
    # Try to find and print context
    idx = content.find("IsInBounds(cell.y, cell.x")
    if idx >= 0:
        print(f"Found at index {idx}: {content[idx:idx+200]}")

# Fix 2: DrawGhostPreview - allow ghost on door border
old2 = """case Tool.PlaceDoor:
            var ghostDoor = new DoorSpawnData { row = row, column = col, direction = doorDirection, doorType = doorType, color = doorColor };
            bool doorValid = !LevelEditorGrid.HasDoorOverlap(ghostDoor, blocks, doors, walls, rows, columns);
            LevelEditorGrid.DrawGhostDoor(gridOrigin, ghostDoor, doorValid, rows, columns);"""

new2 = """case Tool.PlaceDoor:
            if (!LevelEditorGrid.IsDoorInBounds(row, col, rows, columns)) { hoverCell = null; break; }
            var ghostDoor = new DoorSpawnData { row = row, column = col, direction = doorDirection, doorType = doorType, color = doorColor };
            bool doorValid = !LevelEditorGrid.HasDoorOverlap(ghostDoor, blocks, doors, walls, rows, columns);
            LevelEditorGrid.DrawGhostDoor(gridOrigin, ghostDoor, doorValid, rows, columns);"""

if old2 in content:
    content = content.replace(old2, new2)
    print("Fix 2 applied: DrawGhostPreview door border")
else:
    print("ERROR: Fix 2 pattern not found!")

# Fix 3: PlaceDoorAt - allow placement on door border
old3 = """private void PlaceDoorAt(int row, int col)
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
        EditorUtility.DisplayDialog("Cannot Place", "This position overlaps with an existing object or is out of bounds.", "OK");
        return;
    }"""

new3 = """private void PlaceDoorAt(int row, int col)
{
    if (!LevelEditorGrid.IsDoorInBounds(row, col, rows, columns))
    {
        EditorUtility.DisplayDialog("Cannot Place", "Door can only be placed on map border cells.", "OK");
        return;
    }

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
        EditorUtility.DisplayDialog("Cannot Place", "This position overlaps with an existing object.", "OK");
        return;
    }"""

if old3 in content:
    content = content.replace(old3, new3)
    print("Fix 3 applied: PlaceDoorAt border check")
else:
    print("ERROR: Fix 3 pattern not found!")

with open(path, "w", encoding="utf-8") as f:
    f.write(content)

print("Done! File written.")
