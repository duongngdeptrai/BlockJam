using System.Collections.Generic;

public class RemoveObjectCommand<T> : IEditorCommand
{
    private readonly List<T> list;
    private readonly T item;
    private readonly int index;

    public RemoveObjectCommand(List<T> list, T item, int index)
    {
        this.list = list;
        this.item = item;
        this.index = index;
    }

    public void Execute()
    {
        if (index >= 0 && index < list.Count)
        {
            list.RemoveAt(index);
        }
    }

    public void Undo()
    {
        if (index >= 0 && index <= list.Count)
        {
            list.Insert(index, item);
        }
    }
}
