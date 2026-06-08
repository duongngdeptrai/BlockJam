# Unity Level Editor - Implementation Plan

## Context

Project PikaGame29 has a basic `MapEditorWindow.cs` with 6 critical bugs:
1. **No collision detection** - blocks/doors/walls can overlap completely
2. **No bounds checking** - row/column can be outside valid range
3. **Door-block overlap** - doors can be placed on blocks and vice versa
4. **Wall overlap** - walls can overlap anything
5. **Static preview** - no mouse interaction, no click-to-place
6. **No undo/redo** - all mutations are destructive

The plan builds a full replacement with collision detection, undo/redo, drag-and-drop, grid snapping, and preview level.

---

## Files to Create/Modify

### New Files (in `Assets/Scripts/Editor/`)

1. **`EditorCommand.cs`** - Interface base for undo/redo commands
2. **`PlaceObjectCommand.cs`** - Generic command to add objects
3. **`RemoveObjectCommand.cs`** - Generic command to remove objects
4. **`MoveObjectCommand.cs`** - Command to move block to new position
5. **`EditPropertyCommand.cs`** - Command to edit object properties
6. **`ResizeMapCommand.cs`** - Command to resize map with bounds cleanup
7. **`CommandHistory.cs`** - Undo/redo stack manager
8. **`LevelEditorGrid.cs`** - Grid logic: coordinates, footprint, collision, validation, drawing
9. **`LevelEditorPreview.cs`** - Preview level in play mode

### Files to Modify

10. **`MapEditorWindow.cs`** (REWRITE) - Main EditorWindow with interactive grid

### Partial Class Addition

11. **`MapController.GenObject.cs`** - Add `InitMapFromData(LevelData)` method

---

## Implementation Order

### Phase 1: Core Infrastructure (no dependencies on window)
1. `EditorCommand.cs` - interface
2. `PlaceObjectCommand<T>` - generic
3. `RemoveObjectCommand<T>` - generic with deep copy
4. `MoveObjectCommand` - block movement
5. `EditPropertyCommand` - property editing via reflection
6. `ResizeMapCommand` - map resize with out-of-bounds cleanup
7. `CommandHistory` - undo/redo stack

### Phase 2: Grid Logic
8. `LevelEditorGrid` - coordinate conversion, footprint calculation, collision detection, validation, drawing helpers

### Phase 3: Preview System
9. `LevelEditorPreview` - play mode lifecycle
10. Add `InitMapFromData()` to `MapController.GenObject.cs`

### Phase 4: Window Rewrite
11. Rewrite `MapEditorWindow` - full UI with mouse interaction

---

## Key Design Decisions

- **Custom undo/redo** (not Unity's Undo class) - gives full control over editor-only state
- **LevelEditorGrid** as static utility - coordinate math, collision, drawing in one place
- **Footprint-based collision** - each block type has a list of (dx, dy) cell offsets
- **Validation before placement** - ghost preview shows green (valid) or red (overlap)
- **Preview via temp JSON** - serialize to `_preview_temp.json`, load in play mode

---

## Verification Steps

1. Open `Window > PikaGame > Map Editor`
2. Click cells with each tool - verify correct coordinates
3. Place blocks - verify ghost preview, collision blocking
4. Multi-cell blocks (2x3) - verify 6-cell footprint
5. Ctrl+Z/Y - verify undo/redo
6. Click object - verify selection highlight + property inspector
7. Drag block - verify real-time ghost movement
8. Resize slider - verify out-of-bounds objects removed (and restored on undo)
9. Place overlapping objects - verify warning panel
10. Save/Load - verify JSON correctness
11. Preview Level - verify play mode launch and return
