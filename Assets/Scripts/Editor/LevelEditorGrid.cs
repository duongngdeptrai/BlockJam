using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class LevelEditorGrid
{
public const int CellSize = 28;
public const int LabelSize = 18;
public const int BorderPadding = 1;

private static readonly Dictionary<string, List<Vector2Int>> Footprints = new Dictionary<string, List<Vector2Int>>
{
{ "block1x1", new List<Vector2Int> { new Vector2Int(0, 0) } },
{ "block1x2", new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(0, 1) } },
{ "block1x3", new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2) } },
{ "block1x4", new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(0, 3) } },
{ "block2x2", new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) } },
{ "block2x3", new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(0, 2), new Vector2Int(1, 2) } },
{ "blockLShape", new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(1, 2) } },
{ "blockTShape", new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(1, 1) } },
{ "blockZShape", new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1) } },
};

public static Vector2Int ScreenToGrid(Vector2 screenPos, Rect gridOrigin)
{
float localX = screenPos.x - gridOrigin.x;
float localY = screenPos.y - gridOrigin.y;
int col = Mathf.FloorToInt(localX / CellSize);
int row = Mathf.FloorToInt(localY / CellSize);
return new Vector2Int(col, row);
}

public static bool IsInBounds(int row, int col, int rows, int columns)
{
return row >= 0 && row < rows && col >= 0 && col < columns;
}

public static bool IsDoorInBounds(int row, int col, int rows, int columns)
{
return IsInBounds(row, col, rows, columns) || IsDoorBorderCell(row, col, rows, columns);
}

public static bool IsDoorBorderCell(int row, int col, int rows, int columns)
{
bool onTop = row == -1 && col >= 0 && col < columns;
bool onBottom = row == rows && col >= 0 && col < columns;
bool onLeft = col == -1 && row >= 0 && row < rows;
bool onRight = col == columns && row >= 0 && row < rows;
return onTop || onBottom || onLeft || onRight;
}

public static List<Vector2Int> GetBlockFootprint(string blockType)
{
if (Footprints.TryGetValue(blockType, out var footprint))
return footprint;
return new List<Vector2Int> { new Vector2Int(0, 0) };
}

public static List<Vector2Int> GetDoorFootprint(DoorSpawnData door)
{
int size = GetDoorSize(door.doorType);
var footprint = new List<Vector2Int>(size);
for (int i = 0; i < size; i++)
{
footprint.Add(door.direction switch
{
"Up" => new Vector2Int(i, 0),
"Down" => new Vector2Int(i, 0),
"Left" => new Vector2Int(0, i),
"Right" => new Vector2Int(0, i),
_ => new Vector2Int(0, 0),
});
}
return footprint;
}

private static int GetDoorSize(string doorType)
{
return doorType switch
{
"door1" => 1,
"door2" => 2,
"door3" => 3,
"door4" => 4,
_ => 1,
};
}

public static Vector2Int GetBlockDimensions(string blockType)
{
return blockType switch
{
"block1x1" => new Vector2Int(1, 1),
"block1x2" => new Vector2Int(1, 2),
"block1x3" => new Vector2Int(1, 3),
"block1x4" => new Vector2Int(1, 4),
"block2x2" => new Vector2Int(2, 2),
"block2x3" => new Vector2Int(2, 3),
"blockLShape" => new Vector2Int(2, 3),
"blockTShape" => new Vector2Int(3, 2),
"blockZShape" => new Vector2Int(3, 2),
_ => new Vector2Int(1, 1),
};
}

public static List<Vector2Int> GetDoorFootprint(string doorType, string direction)
{
int size = GetDoorSize(doorType);
var footprint = new List<Vector2Int>();
for (int i = 0; i < size; i++)
{
footprint.Add(direction switch
{
"Up" => new Vector2Int(i, 0),
"Down" => new Vector2Int(i, 0),
"Left" => new Vector2Int(0, i),
"Right" => new Vector2Int(0, i),
_ => new Vector2Int(0, 0),
});
}
return footprint;
}

// ==================== COLLISION DETECTION ====================

public static bool HasBlockOverlap(BlockData block, List<BlockData> blocks, List<DoorSpawnData> doors, List<WallSpawnData> walls, int rows, int columns)
{
var footprint = GetBlockFootprint(block.blockType);
foreach (var offset in footprint)
{
int r = block.row + offset.y;
int c = block.column + offset.x;
if (!IsInBounds(r, c, rows, columns))
return true;
foreach (var other in blocks)
{
var otherFootprint = GetBlockFootprint(other.blockType);
foreach (var otherOffset in otherFootprint)
{
if (other.row + otherOffset.y == r && other.column + otherOffset.x == c)
return true;
}
}
foreach (var door in doors)
{
if (door.row == r && door.column == c)
return true;
}
foreach (var wall in walls)
{
if (wall.row == r && wall.column == c)
return true;
}
}
return false;
}

public static bool HasDoorOverlap(DoorSpawnData door, List<BlockData> blocks, List<DoorSpawnData> doors, List<WallSpawnData> walls, int rows, int columns)
{
var doorFootprint = GetDoorFootprint(door.doorType, door.direction);
foreach (var offset in doorFootprint)
{
int r = door.row + offset.y;
int c = door.column + offset.x;
if (!IsDoorInBounds(r, c, rows, columns))
return true;
foreach (var block in blocks)
{
var footprint = GetBlockFootprint(block.blockType);
foreach (var blockOffset in footprint)
{
if (block.row + blockOffset.y == r && block.column + blockOffset.x == c)
return true;
}
}
foreach (var other in doors)
{
if (other == door) continue;
var otherFootprint = GetDoorFootprint(other.doorType, other.direction);
foreach (var otherOffset in otherFootprint)
{
if (other.row + otherOffset.y == r && other.column + otherOffset.x == c)
return true;
}
}
foreach (var wall in walls)
{
if (wall.row == r && wall.column == c)
return true;
}
}
return false;
}

public static bool HasWallOverlap(WallSpawnData wall, List<BlockData> blocks, List<DoorSpawnData> doors, List<WallSpawnData> walls, int rows, int columns)
{
if (!IsInBounds(wall.row, wall.column, rows, columns))
return true;
foreach (var block in blocks)
{
var footprint = GetBlockFootprint(block.blockType);
foreach (var offset in footprint)
{
if (block.row + offset.y == wall.row && block.column + offset.x == wall.column)
return true;
}
}
foreach (var door in doors)
{
if (door.row == wall.row && door.column == wall.column)
return true;
}
foreach (var other in walls)
{
if (other.row == wall.row && other.column == wall.column)
return true;
}
return false;
}

// ==================== VALIDATION ====================

public static List<string> GetAllValidationErrors(LevelData level)
{
var errors = new List<string>();
if (level == null) return errors;

for (int i = 0; i < level.blocks.Count; i++)
{
var block = level.blocks[i];
var footprint = GetBlockFootprint(block.blockType);
foreach (var offset in footprint)
{
int r = block.row + offset.y;
int c = block.column + offset.x;
if (!IsInBounds(r, c, level.rows, level.columns))
{
errors.Add($"Block #{i} ({block.blockType}) at ({block.row},{block.column}) out of bounds at cell ({r},{c})");
break;
}
}
for (int j = i + 1; j < level.blocks.Count; j++)
{
var other = level.blocks[j];
if (BlocksOverlap(block, other))
{
errors.Add($"Block #{i} ({block.blockType}) at ({block.row},{block.column}) overlaps with Block #{j} ({other.blockType}) at ({other.row},{other.column})");
break;
}
}
foreach (var door in level.doors)
{
if (BlockOverlapsDoor(block, door))
{
errors.Add($"Block #{i} ({block.blockType}) at ({block.row},{block.column}) overlaps with Door {door.doorType} at ({door.row},{door.column})");
break;
}
}
foreach (var wall in level.walls)
{
if (BlockOverlapsWall(block, wall))
{
errors.Add($"Block #{i} ({block.blockType}) at ({block.row},{block.column}) overlaps with Wall at ({wall.row},{wall.column})");
break;
}
}
}

for (int i = 0; i < level.doors.Count; i++)
{
var door = level.doors[i];
if (!IsDoorInBounds(door.row, door.column, level.rows, level.columns))
{
errors.Add($"Door {door.doorType} at ({door.row},{door.column}) out of bounds");
}
for (int j = i + 1; j < level.doors.Count; j++)
{
var other = level.doors[j];
var footA = GetDoorFootprint(door.doorType, door.direction);
var footB = GetDoorFootprint(other.doorType, other.direction);
bool hit = false;
foreach (var oA in footA)
{
foreach (var oB in footB)
{
if (door.row + oA.y == other.row + oB.y && door.column + oA.x == other.column + oB.x)
{ hit = true; break; }
}
if (hit) break;
}
if (hit)
{
errors.Add($"Door {door.doorType} at ({door.row},{door.column}) overlaps with Door {other.doorType}");
break;
}
}
var wallFootprint = GetDoorFootprint(door.doorType, door.direction);
foreach (var wall in level.walls)
{
bool wallHit = false;
foreach (var dOff in wallFootprint)
{
if (door.row + dOff.y == wall.row && door.column + dOff.x == wall.column)
{ wallHit = true; break; }
}
if (wallHit)
{
errors.Add($"Door {door.doorType} at ({door.row},{door.column}) overlaps with Wall");
break;
}
}
}

for (int i = 0; i < level.walls.Count; i++)
{
var wall = level.walls[i];
if (!IsInBounds(wall.row, wall.column, level.rows, level.columns))
{
errors.Add($"Wall at ({wall.row},{wall.column}) out of bounds");
}
for (int j = i + 1; j < level.walls.Count; j++)
{
var other = level.walls[j];
if (wall.row == other.row && wall.column == other.column)
{
errors.Add($"Wall at ({wall.row},{wall.column}) overlaps with another Wall");
break;
}
}
}

return errors;
}

private static bool BlocksOverlap(BlockData a, BlockData b)
{
var footprintA = GetBlockFootprint(a.blockType);
var footprintB = GetBlockFootprint(b.blockType);
foreach (var offsetA in footprintA)
{
int rA = a.row + offsetA.y;
int cA = a.column + offsetA.x;
foreach (var offsetB in footprintB)
{
if (b.row + offsetB.y == rA && b.column + offsetB.x == cA)
return true;
}
}
return false;
}

private static bool BlockOverlapsDoor(BlockData block, DoorSpawnData door)
{
var footprint = GetBlockFootprint(block.blockType);
foreach (var offset in footprint)
{
if (block.row + offset.y == door.row && block.column + offset.x == door.column)
return true;
}
return false;
}

private static bool BlockOverlapsWall(BlockData block, WallSpawnData wall)
{
var footprint = GetBlockFootprint(block.blockType);
foreach (var offset in footprint)
{
if (block.row + offset.y == wall.row && block.column + offset.x == wall.column)
return true;
}
return false;
}

// ==================== DRAWING HELPERS ====================

public static void DrawGridBackground(Rect gridOrigin, int rows, int columns)
{
Color borderColor = new Color(0.28f, 0.28f, 0.28f);
Color innerColor = new Color(0.18f, 0.18f, 0.18f);
Color outerColor = new Color(0.10f, 0.10f, 0.10f);

int previewRows = rows + 2;
int previewColumns = columns + 2;

for (int visualRow = 0; visualRow < previewRows; visualRow++)
{
int mapRow = visualRow - 1;
for (int visualCol = 0; visualCol < previewColumns; visualCol++)
{
int mapCol = visualCol - 1;
Rect cellRect = new Rect(
gridOrigin.x + visualCol * CellSize,
gridOrigin.y + visualRow * CellSize,
CellSize, CellSize);
bool insidePlayable = IsInBounds(mapRow, mapCol, rows, columns);
EditorGUI.DrawRect(cellRect, insidePlayable ? innerColor : outerColor);
DrawCellBorder(cellRect, borderColor);
}
}

GUIStyle indexStyle = EditorStyles.centeredGreyMiniLabel;
for (int visualCol = 0; visualCol < previewColumns; visualCol++)
{
int mapCol = visualCol - 1;
Rect labelRect = new Rect(
gridOrigin.x + visualCol * CellSize,
gridOrigin.y - LabelSize,
CellSize, LabelSize);
GUI.Label(labelRect, mapCol.ToString(), indexStyle);
}
for (int visualRow = 0; visualRow < previewRows; visualRow++)
{
int mapRow = visualRow - 1;
Rect labelRect = new Rect(
gridOrigin.x - LabelSize,
gridOrigin.y + visualRow * CellSize,
LabelSize, CellSize);
GUI.Label(labelRect, mapRow.ToString(), indexStyle);
}
}

public static void DrawBlockFootprint(Rect gridOrigin, BlockData block, int rows, int columns)
{
var footprint = GetBlockFootprint(block.blockType);
Color fillColor = GetEditorColor(block.color);
for (int i = 0; i < footprint.Count; i++)
{
Vector2Int offset = footprint[i];
int mapRow = block.row + offset.y;
int mapColumn = block.column + offset.x;
if (!IsInBounds(mapRow, mapColumn, rows, columns)) continue;
int visualRow = mapRow + 1;
int visualCol = mapColumn + 1;
Rect cellRect = new Rect(
gridOrigin.x + visualCol * CellSize,
gridOrigin.y + visualRow * CellSize,
CellSize, CellSize);
EditorGUI.DrawRect(cellRect, fillColor);
DrawCellBorder(cellRect, Color.white);
}
if (IsInBounds(block.row, block.column, rows, columns))
{
int visualRow = block.row + 1;
int visualCol = block.column + 1;
Rect anchorRect = new Rect(
gridOrigin.x + visualCol * CellSize,
gridOrigin.y + visualRow * CellSize,
CellSize, CellSize);
GUI.Label(anchorRect, GetBlockLabel(block.blockType), EditorStyles.centeredGreyMiniLabel);
}
}

public static void DrawDoor(Rect gridOrigin, DoorSpawnData door, int rows, int columns)
{
var footprint = GetDoorFootprint(door.doorType, door.direction);
bool anyOutOfBounds = false;
foreach (var offset in footprint)
{
int r = door.row + offset.y;
int c = door.column + offset.x;
if (!IsDoorInBounds(r, c, rows, columns)) { anyOutOfBounds = true; break; }
}
if (anyOutOfBounds) return;

foreach (var offset in footprint)
{
int mapRow = door.row + offset.y;
int mapColumn = door.column + offset.x;
int visualRow = mapRow + 1;
int visualCol = mapColumn + 1;
Rect doorRect = new Rect(
gridOrigin.x + visualCol * CellSize,
gridOrigin.y + visualRow * CellSize,
CellSize, CellSize);
Color baseColor = GetEditorColor(door.color);
Color fillColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.70f);
EditorGUI.DrawRect(doorRect, fillColor);
DrawCellBorder(doorRect, Color.white);
Rect innerRect = new Rect(doorRect.x + 3, doorRect.y + 3, doorRect.width - 6, doorRect.height - 6);
EditorGUI.DrawRect(innerRect, new Color(0f, 0f, 0f, 0.18f));
}

int anchorVisualRow = door.row + 1;
int anchorVisualCol = door.column + 1;
Rect anchorRect = new Rect(
gridOrigin.x + anchorVisualCol * CellSize,
gridOrigin.y + anchorVisualRow * CellSize,
CellSize, CellSize);
string label = GetDoorShortLabel(door);
GUI.Label(anchorRect, label, EditorStyles.centeredGreyMiniLabel);

string dir = door.direction switch { "Up" => "^", "Down" => "v", "Left" => "<", "Right" => ">", _ => "?" };
Rect dirRect = new Rect(anchorRect.x, anchorRect.y + anchorRect.height * 0.5f, anchorRect.width, anchorRect.height * 0.5f);
GUI.Label(dirRect, dir, EditorStyles.miniLabel);
}

public static void DrawWall(Rect gridOrigin, WallSpawnData wall, int rows, int columns)
{
int mapRow = wall.row;
int mapColumn = wall.column;
if (!IsInBounds(mapRow, mapColumn, rows, columns)) return;
int visualRow = mapRow + 1;
int visualCol = mapColumn + 1;
Rect wallRect = new Rect(
gridOrigin.x + visualCol * CellSize,
gridOrigin.y + visualRow * CellSize,
CellSize, CellSize);
EditorGUI.DrawRect(wallRect, new Color(0.5f, 0.5f, 0.5f, 0.8f));
DrawCellBorder(wallRect, Color.gray);
string dir = wall.direction switch { "Up" => "^", "Down" => "v", "Left" => "<", "Right" => ">", _ => "?" };
GUI.Label(wallRect, dir, EditorStyles.centeredGreyMiniLabel);
}

public static void DrawSelectionHighlight(Rect gridOrigin, List<Vector2Int> footprint, int rows, int columns)
{
foreach (var offset in footprint)
{
int r = offset.y;
int c = offset.x;
if (!IsInBounds(r, c, rows, columns)) continue;
int visualRow = r + 1;
int visualCol = c + 1;
Rect cellRect = new Rect(
gridOrigin.x + visualCol * CellSize,
gridOrigin.y + visualRow * CellSize,
CellSize, CellSize);
EditorGUI.DrawRect(cellRect, new Color(1f, 1f, 0.3f, 0.35f));
DrawCellBorder(cellRect, Color.yellow);
}
}

public static void DrawGhostFootprint(Rect gridOrigin, BlockData ghostBlock, bool isValid, int rows, int columns)
{
var footprint = GetBlockFootprint(ghostBlock.blockType);
bool allValid = isValid;
foreach (var offset in footprint)
{
int r = ghostBlock.row + offset.y;
int c = ghostBlock.column + offset.x;
if (!IsInBounds(r, c, rows, columns))
allValid = false;
}
Color color = allValid ? new Color(0.2f, 1f, 0.2f, 0.35f) : new Color(1f, 0.2f, 0.2f, 0.35f);
Color border = allValid ? Color.green : Color.red;

foreach (var offset in footprint)
{
int r = ghostBlock.row + offset.y;
int c = ghostBlock.column + offset.x;
int visualRow = r + 1;
int visualCol = c + 1;
Rect cellRect = new Rect(
gridOrigin.x + visualCol * CellSize,
gridOrigin.y + visualRow * CellSize,
CellSize, CellSize);
bool cellInBounds = IsInBounds(r, c, rows, columns);
if (!cellInBounds)
{
EditorGUI.DrawRect(cellRect, new Color(1f, 0.2f, 0.2f, 0.35f));
DrawCellBorder(cellRect, Color.red);
}
else
{
EditorGUI.DrawRect(cellRect, color);
DrawCellBorder(cellRect, border);
}
}
}

public static void DrawGhostDoor(Rect gridOrigin, DoorSpawnData ghostDoor, bool isValid, int rows, int columns)
{
var footprint = GetDoorFootprint(ghostDoor.doorType, ghostDoor.direction);
bool allValid = isValid;
foreach (var offset in footprint)
{
int r = ghostDoor.row + offset.y;
int c = ghostDoor.column + offset.x;
bool inPlayable = IsInBounds(r, c, rows, columns);
bool onBorder = IsDoorBorderCell(r, c, rows, columns);
if (!inPlayable && !onBorder) allValid = false;
}
Color fill = allValid ? new Color(0.2f, 0.8f, 1f, 0.35f) : new Color(1f, 0.2f, 0.2f, 0.35f);
Color border = allValid ? Color.cyan : Color.red;
for (int i = 0; i < footprint.Count; i++)
{
Vector2Int offset = footprint[i];
int r = ghostDoor.row + offset.y;
int c = ghostDoor.column + offset.x;
int visualRow = r + 1;
int visualCol = c + 1;
Rect cellRect = new Rect(
gridOrigin.x + visualCol * CellSize,
gridOrigin.y + visualRow * CellSize,
CellSize, CellSize);
bool cellOk = IsInBounds(r, c, rows, columns) || IsDoorBorderCell(r, c, rows, columns);
if (!cellOk)
{
EditorGUI.DrawRect(cellRect, new Color(1f, 0.2f, 0.2f, 0.35f));
DrawCellBorder(cellRect, Color.red);
}
else
{
EditorGUI.DrawRect(cellRect, fill);
DrawCellBorder(cellRect, border);
}
}
}

public static void DrawGhostWall(Rect gridOrigin, WallSpawnData ghostWall, bool isValid, int rows, int columns)
{
int r = ghostWall.row;
int c = ghostWall.column;
bool inBounds = IsInBounds(r, c, rows, columns);
if (!inBounds) return;
int visualRow = r + 1;
int visualCol = c + 1;
Rect cellRect = new Rect(
gridOrigin.x + visualCol * CellSize,
gridOrigin.y + visualRow * CellSize,
CellSize, CellSize);
Color fill = isValid ? new Color(0.6f, 0.6f, 0.6f, 0.35f) : new Color(1f, 0.2f, 0.2f, 0.35f);
Color border = isValid ? Color.gray : Color.red;
EditorGUI.DrawRect(cellRect, fill);
DrawCellBorder(cellRect, border);
}

// ==================== UTILITIES ====================

public static void DrawCellBorder(Rect rect, Color borderColor)
{
EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, BorderPadding), borderColor);
EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - BorderPadding, rect.width, BorderPadding), borderColor);
EditorGUI.DrawRect(new Rect(rect.x, rect.y, BorderPadding, rect.height), borderColor);
EditorGUI.DrawRect(new Rect(rect.xMax - BorderPadding, rect.y, BorderPadding, rect.height), borderColor);
}

private static Color GetEditorColor(string colorName)
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
_ => Color.white,
};
}

private static string GetBlockLabel(string blockType)
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
_ => "?",
};
}

private static string GetDoorShortLabel(DoorSpawnData door)
{
string dir = door.direction switch { "Up" => "^", "Down" => "v", "Left" => "<", "Right" => ">", _ => "?" };
return string.IsNullOrEmpty(door.doorType) ? dir : $"{door.doorType.Replace("door","")} {dir}";
}

public static Rect GetGridRect(int rows, int columns)
{
int previewRows = rows + 2;
int previewColumns = columns + 2;
return new Rect(0, 0, LabelSize + previewColumns * CellSize, LabelSize + previewRows * CellSize);
}

public static Rect GetGridOrigin(Rect previewArea)
{
return new Rect(previewArea.x + LabelSize, previewArea.y + LabelSize, 0, 0);
}
}
