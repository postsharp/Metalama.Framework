internal class TargetClass
{
    private EventHandler? _field;

    [Test]
    public event EventHandler Event
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

    private event EventHandler Event_Source
    {
        add => _field += value;
        remove => _field -= value;
    }
    public event EventHandler? EventField
    {
        add
        {
            global::System.Console.WriteLine("Override");
            this.EventField_Source += value;

        }
        remove
        {
            global::System.Console.WriteLine("Override");
            this.EventField_Source -= value;
        }
    }

    private EventHandler? EventField_Source;
}