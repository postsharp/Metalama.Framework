[TestOutput]
[Retry]
class Program
{
    private void IntroducedMethod1(global::System.String name)
    {
        this.MyEvent?.Invoke(this, new global::System.ComponentModel.PropertyChangedEventArgs(name));
        this.MyEvent(this, new global::System.ComponentModel.PropertyChangedEventArgs(name));
    }

    private event global::System.ComponentModel.PropertyChangedEventHandler? MyEvent;
}