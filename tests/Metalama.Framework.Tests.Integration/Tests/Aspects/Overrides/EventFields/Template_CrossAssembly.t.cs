[TestAspect]
internal class TargetClass
{

    private EventHandler? _event;

    public event EventHandler? Event
    {
        add
        {
            global::System.Console.WriteLine("Aspect code");
            this._event += value;
            return;
        }
        remove
        {
            global::System.Console.WriteLine("Aspect code");
            this._event -= value;
            return;
        }
    }
}