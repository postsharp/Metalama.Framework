[Introduction]
    internal class TargetClass { 

public event global::System.EventHandler? Event
{
    add
    {
        global::System.Console.WriteLine("This is the add template.");
        global::System.Console.WriteLine("Original add accessor.");
    }

    remove
    {
        global::System.Console.WriteLine("This is the remove template.");
        global::System.Console.WriteLine("Original remove accessor.");
    }
}}