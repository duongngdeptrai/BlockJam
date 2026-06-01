using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ResizeMapCommand : IEditorCommand
{
    private readonly MapEditorWindow window;
    private readonly int oldRows;
    private readonly int newRows;
    private readonly int oldColumns;
    private readonly int newColumns;
    private readonly float oldTimeLimit;
    private readonly float newTimeLimit;

    // Removed objects due to shrinking
    private readonly List<BlockData> removedBlocks = new List<BlockData>();
    private readonly List<DoorSpawnData> removedDoors = new List<DoorSpawnData>();
    private readonly List<WallSpawnData> removedWalls = new List<WallSpawnData>();

    public ResizeMapCommand(MapEditorWindow window, int newRows, int newColumns, float newTimeLimit)
    {
        this.window = window;
        this.oldRows = window.Rows;
        this.oldColumns = window.Columns;
        this.oldTimeLimit = window.TimeLimit;
        this.newRows = newRows;
        this.newColumns = newColumns;
        this.newTimeLimit = newTimeLimit;

        // Backup objects that will be removed
        foreach (var b in window.Blocks)
        {
            if (b.row >= newRows || b.column >= newColumns)
                removedBlocks.Add(JsonUtility.FromJson<BlockData>(JsonUtility.ToJson(b)));
        }
        foreach (var d in window.Doors)
        {
            if (d.row >= newRows || d.column >= newColumns)
                removedDoors.Add(JsonUtility.FromJson<DoorSpawnData>(JsonUtility.ToJson(d)));
        }
        foreach (var w in window.Walls)
        {
            if (w.row >= newRows || w.column >= newColumns)
                removedWalls.Add(JsonUtility.FromJson<WallSpawnData>(JsonUtility.ToJson(w)));
        }
    }

    public void Execute()
    {
        window.SetRows(newRows);
        window.SetColumns(newColumns);
        window.SetTimeLimit(newTimeLimit);

        // Remove out-of-bounds objects
        window.Blocks.RemoveAll(b => b.row >= newRows || b.column >= newColumns);
        window.Doors.RemoveAll(d => d.row >= newRows || d.column >= newColumns);
        window.Walls.RemoveAll(w => w.row >= newRows || w.column >= newColumns);
    }

    public void Undo()
    {
        window.SetRows(oldRows);
        window.SetColumns(oldColumns);
        window.SetTimeLimit(oldTimeLimit);

        // Restore removed objects
        foreach (var b in removedBlocks)
            window.Blocks.Add(b);
        foreach (var d in removedDoors)
            window.Doors.Add(d);
        foreach (var w in removedWalls)
            window.Walls.Add(w);
    }
}
