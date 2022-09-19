[Retry]
    internal class Program { 

private event global::System.ComponentModel.PropertyChangedEventHandler? MyEvent;

private void IntroducedMethod1(global::System.String name)
{
    MyEvent?.Invoke(this, new global::System.ComponentModel.PropertyChangedEventArgs(name));
    MyEvent(this, new global::System.ComponentModel.PropertyChangedEventArgs(name));
}}