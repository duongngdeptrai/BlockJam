import re

path = r"d:\GameUnity\PikaGame29\Assets\Scripts\Editor\LevelEditorGrid.cs"
with open(path, "r", encoding="utf-8") as f:
    content = f.read()

# Fix door label in DrawDoor
old1 = 'string label = string.IsNullOrEmpty(door.doorType) ? GetDoorLabel(door) : door.doorType;\n        GUI.Label(doorRect, label, EditorStyles.boldLabel);'
new1 = 'string label = GetDoorShortLabel(door);\n        GUI.Label(doorRect, label, EditorStyles.centeredGreyMiniLabel);'
if old1 in content:
    content = content.replace(old1, new1, 1)
    print("Fix door label: OK")
else:
    print("Fix door label: NOT FOUND")

# Fix GetDoorLabel -> rename to GetDoorShortLabel
old2 = 'private static string GetDoorLabel(DoorSpawnData door)'
new2 = 'private static string GetDoorShortLabel(DoorSpawnData door)'
if old2 in content:
    content = content.replace(old2, new2, 1)
    print("Rename GetDoorLabel: OK")
else:
    print("Rename GetDoorLabel: NOT FOUND")

# Update the return in GetDoorLabel/GetDoorShortLabel to show short label
old3 = 'return string.IsNullOrEmpty(door.doorType) ? dir : $"{door.doorType}\\n{dir}";'
new3 = 'return string.IsNullOrEmpty(door.doorType) ? dir : $"{door.doorType.Replace("door","")} {dir}";'
if old3 in content:
    content = content.replace(old3, new3, 1)
    print("Fix label format: OK")
else:
    print("Fix label format: NOT FOUND")

with open(path, "w", encoding="utf-8") as f:
    f.write(content)
print("Done")
