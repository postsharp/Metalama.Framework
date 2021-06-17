[TestOutput]
internal class TargetClass
{
    private HashSet<EventHandler> handlers = new HashSet<EventHandler>();
    [Override]
    public event EventHandler Event
    {
        add
        {
            global::System.Console.WriteLine("This is the add template.");
            global::System.EventHandler __;
            Console.WriteLine("This is the original add.");
            this.handlers.Add(value);
        }

        remove
        {
            global::System.Console.WriteLine("This is the remove template.");
            global::System.EventHandler ___1;
            Console.WriteLine("This is the original remove.");
            this.handlers.Remove(value);
        }
    }
}