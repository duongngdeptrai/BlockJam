using System.Collections.Generic;

public class CommandHistory
{
    private readonly Stack<IEditorCommand> undoStack = new Stack<IEditorCommand>();
    private readonly Stack<IEditorCommand> redoStack = new Stack<IEditorCommand>();
    private const int MaxHistory = 100;

    public bool CanUndo => undoStack.Count > 0;
    public bool CanRedo => redoStack.Count > 0;

    public void Execute(IEditorCommand command)
    {
        command.Execute();
        undoStack.Push(command);
        redoStack.Clear();

        if (undoStack.Count > MaxHistory)
            undoStack.Clear();
    }

    public void Undo()
    {
        if (!CanUndo) return;
        IEditorCommand command = undoStack.Pop();
        command.Undo();
        redoStack.Push(command);
    }

    public void Redo()
    {
        if (!CanRedo) return;
        IEditorCommand command = redoStack.Pop();
        command.Execute();
        undoStack.Push(command);
    }

    public void Clear()
    {
        undoStack.Clear();
        redoStack.Clear();
    }
}
