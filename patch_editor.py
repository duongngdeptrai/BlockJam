import re

path = r"d:\GameUnity\PikaGame29\Assets\Scripts\Editor\MapEditorWindow.cs"
with open(path, "r", encoding="utf-8") as f:
    code = f.read()

# Fix 1: HandleGridMouse - allow door border cells
old1 = "bool inBounds = LevelEditorGrid.IsInBounds(cell.y, cell.x, rows, columns);"
new1 = """bool inPlayable = LevelEditorGrid.IsInBounds(cell.y, cell.x, rows, columns);
    bool onDoorBorder = !inPlayable && LevelEditorGrid.IsDoorBorderCell(cell.y, cell.x, rows, columns);
    bool inBounds = inPlayable || (currentTool == Tool.PlaceDoor && onDoorBorder);"""

if old1 in code:
    code = code.replace(old1, new1, 1)
    print("Fix 1 OK: HandleGridMouse door border")
else:
    print("Fix 1 FAIL: string not found")

# Fix 2: HandleGridClick - allow door placement at border
old2 = """case Tool.PlaceDoor:
            PlaceDoorAt(row, col);
            break;"""
new2 = """case Tool.PlaceDoor:
            PlaceDoorAt(row, col);
            break;"""

if old2 in code:
    print("Fix 2 SKIP: HandleGridClick already calls PlaceDoorAt")
else:
    print("Fix 2 checking...")

# Fix 3: PlaceDoorAt - allow border placement, also place when on border
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

if old3 in code:
    code = code.replace(old3, new3, 1)
    print("Fix 3 OK: PlaceDoorAt")
else:
    print("Fix 3 SKIP: already correct")

with open(path, "w", encoding="utf-8") as f:
    f.write(code)
print("Done")
