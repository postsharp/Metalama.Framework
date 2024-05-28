// Final Compilation.Emit failed.
// Error CS0738 on `global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.IncompatibleMembers.IInterface`: `'TargetClass' does not implement interface member 'IInterface.Event'. 'TargetClass.Event' cannot implement 'IInterface.Event' because it does not have the matching return type of 'EventHandler'.`
// Error CS0738 on `global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.IncompatibleMembers.IInterface`: `'TargetClass' does not implement interface member 'IInterface.Method()'. 'TargetClass.Method()' cannot implement 'IInterface.Method()' because it does not have the matching return type of 'int'.`
// Error CS0738 on `global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.IncompatibleMembers.IInterface`: `'TargetClass' does not implement interface member 'IInterface.Property'. 'TargetClass.Property' cannot implement 'IInterface.Property' because it does not have the matching return type of 'int'.`
[Introduction]
public class TargetClass : global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.IncompatibleMembers.IInterface
{
    public global::System.Int64 Property
    {
        get
        {
            global::System.Console.WriteLine("This is introduced interface member.");
            return (global::System.Int64)42;
        }
        set
        {
            global::System.Console.WriteLine("This is introduced interface member.");
        }
    }
    public global::System.Int64 Method()
    {
        global::System.Console.WriteLine("This is introduced interface member.");
        return default(global::System.Int64);
    }
    public event global::System.UnhandledExceptionEventHandler? Event
    {
        add
        {
            global::System.Console.WriteLine("This is introduced interface member.");
        }
        remove
        {
            global::System.Console.WriteLine("This is introduced interface member.");
        }
    }
}