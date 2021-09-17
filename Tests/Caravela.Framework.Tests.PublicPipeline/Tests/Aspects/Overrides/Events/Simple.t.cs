    internal class TargetClass
    {
        private HashSet<EventHandler> handlers = new HashSet<EventHandler>();

        [Override]
        public event EventHandler Event
{add    {
        global::System.Console.WriteLine("This is the add template.");
                Console.WriteLine("This is the original add.");
                this.handlers.Add(value);
    }

remove    {
        global::System.Console.WriteLine("This is the remove template.");
                Console.WriteLine("This is the original remove.");
                this.handlers.Remove(value);
    }
}
    }