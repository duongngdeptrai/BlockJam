using System.Collections.Generic;

public class MoveObjectCommand : IEditorCommand
{
    private readonly List<BlockData> blocks;
    private readonly int index;
    private readonly int oldRow;
    private readonly int oldColumn;
    private readonly int newRow;
    private readonly int newColumn;

    public MoveObjectCommand(List<BlockData> blocks, int index, int oldRow, int oldColumn, int newRow, int newColumn)
    {
        this.blocks = blocks;
        this.index = index;
        this.oldRow = oldRow;
        this.oldColumn = oldColumn;
        this.newRow = newRow;
        this.newColumn = newColumn;
    }

    public void Execute()
    {
        if (index >= 0 && index < blocks.Count)
        {
            blocks[index].row = newRow;
            blocks[index].column = newColumn;
        }
    }

    public void Undo()
    {
        if (index >= 0 && index < blocks.Count)
        {
            blocks[index].row = oldRow;
            blocks[index].column = oldColumn;
        }
    }
}
