internal class TargetClass
{
    private EventHandler? _field;

    [Test]
    public event EventHandler Event
    {
        add
        {
            this.Event_Source += value;


        }
        remove
        {
            this.Event_Source -= value;

        }
    }

    private event EventHandler Event_Source
    {
        add => this._field += value;
        remove => this._field -= value;
    }
    public event EventHandler? EventField
    {
        add
        {
            this.EventField_Source += value;

        }
        remove
        {
            this.EventField_Source -= value;
        }
    }

    private EventHandler? EventField_Source;
}