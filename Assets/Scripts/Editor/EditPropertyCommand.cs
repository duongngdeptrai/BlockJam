using System.Reflection;

public class EditPropertyCommand : IEditorCommand
{
    private readonly object target;
    private readonly FieldInfo field;
    private readonly object oldValue;
    private readonly object newValue;

    public EditPropertyCommand(object target, string fieldName, object newValue)
    {
        this.target = target;
        this.field = target.GetType().GetField(fieldName);
        this.oldValue = field?.GetValue(target);
        this.newValue = newValue;
    }

    public void Execute()
    {
        field?.SetValue(target, newValue);
    }

    public void Undo()
    {
        field?.SetValue(target, oldValue);
    }
}
