[TestIntroduction]
[Override]
internal class TargetClass
{
    private global::System.EventHandler? _event;



    public event global::System.EventHandler? Event
    {
        add
        {
            global::System.Console.WriteLine("Override");
            this.Event_Source += value;
        }
        remove
        {
            global::System.Console.WriteLine("Override");
            this.Event_Source -= value;
        }
    }

    private event global::System.EventHandler? Event_Source
    {
        add
        {
            this._event += value;
        }

        remove
        {
            this._event -= value;
        }
    }
}