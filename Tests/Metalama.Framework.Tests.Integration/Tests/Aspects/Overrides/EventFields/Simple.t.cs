    internal class TargetClass
    {

private EventHandler? _event;

        public event EventHandler? Event{add    {
        global::System.Console.WriteLine("This is the add template.");
this._event+=value;    }

remove    {
        global::System.Console.WriteLine("This is the remove template.");
this._event-=value;    }
}
    }