path = r"d:\GameUnity\PikaGame29\Assets\Scripts\Editor\LevelEditorGrid.cs"
with open(path, "r", encoding="utf-8") as f:
    lines = f.readlines()

# Print lines around the target for debugging
for i in range(64, 76):
    print(f"{i+1}: {repr(lines[i])}")
