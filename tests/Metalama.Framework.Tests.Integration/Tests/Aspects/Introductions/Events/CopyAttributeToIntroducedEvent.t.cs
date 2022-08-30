[Introduction]
internal class TargetClass
{
    [Override]
    public event global::System.EventHandler? Event
    {
        add
        {
            global::System.Console.WriteLine("This is the overriden add template.");
            global::System.Console.WriteLine("Original add accessor.");
        }

        remove
        {
            global::System.Console.WriteLine("This is the overriden remove template.");
            global::System.Console.WriteLine("Original remove accessor.");
        }
    }
}