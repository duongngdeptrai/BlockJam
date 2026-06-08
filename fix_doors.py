import re

path = r"d:\GameUnity\PikaGame29\Assets\Scripts\Editor\MapEditorWindow.cs"
with open(path, "r", encoding="utf-8") as f:
    content = f.read()

print(f"File length: {len(content)} chars")

# Check the exact patterns
old1 = "bool inBounds = LevelEditorGrid.IsInBounds(cell.y, cell.x, rows, columns);"
if old1 in content:
    print("Fix 1 pattern found")
    new1 = (
        "bool inPlayable = LevelEditorGrid.IsInBounds(cell.y, cell.x, rows, columns);\n"
        "    bool onDoorBorder = !inPlayable && LevelEditorGrid.IsDoorBorderCell(cell.y, cell.x, rows, columns);\n"
        "    bool inBounds = inPlayable || (currentTool == Tool.PlaceDoor \x26\x26 onDoorBorder);"
    )
    content = content.replace(old1, new1, 1)
else:
    print("Fix 1 pattern NOT found")

old2 = "bool inBounds = IsInBounds(r, c, rows, columns);\n        if (!inBounds) return;"
if old2 in content:
    print("Fix 2 pattern found")
    new2 = (
        "bool inPlayable = IsInBounds(r, c, rows, columns);\n"
        "        bool onBorder = IsDoorBorderCell(r, c, rows, columns);\n"
        "        if (!inPlayable \x26\x26 !onBorder) return;"
    )
    content = content.replace(old2, new2, 1)
else:
    print("Fix 2 pattern NOT found")

with open(path, "w", encoding="utf-8") as f:
    f.write(content)
print("Done")
