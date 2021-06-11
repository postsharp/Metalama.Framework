[TestOutput]
[Retry]
class Program
{
    private void IntroducedMethod1(string name)
    {
        this.MyEvent?.Invoke(this, new global::System.ComponentModel.PropertyChangedEventArgs(name));
        this.MyEvent(this, new global::System.ComponentModel.PropertyChangedEventArgs(name));
    }

    private event global::System.ComponentModel.PropertyChangedEventHandler? MyEvent;
}