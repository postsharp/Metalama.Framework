[Introduction]
public class TargetClass : global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Tags.IInterface1, global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Tags.IInterface2
{
    public global::System.Int32 Property1
    {
        get
        {
            global::System.Console.WriteLine("This introduced member has Tag? False.");
            return (global::System.Int32)42;
        }
        set
        {
            global::System.Console.WriteLine("This introduced member has Tag? False.");
        }
    }
    public global::System.Int32 Property2
    {
        get
        {
            global::System.Console.WriteLine("This introduced member has Tag? False.");
            return (global::System.Int32)42;
        }
        set
        {
            global::System.Console.WriteLine("This introduced member has Tag? False.");
        }
    }
    public global::System.Int32 InterfaceMethod1()
    {
        global::System.Console.WriteLine("This introduced member has Tag? False.");
        return default(global::System.Int32);
    }
    public global::System.Int32 InterfaceMethod2()
    {
        global::System.Console.WriteLine("This introduced member has Tag? False.");
        return default(global::System.Int32);
    }
    public event global::System.EventHandler? Event1
    {
        add
        {
            global::System.Console.WriteLine("This introduced member has Tag? False.");
        }
        remove
        {
            global::System.Console.WriteLine("This introduced member has Tag? False.");
        }
    }
    public event global::System.EventHandler? Event2
    {
        add
        {
            global::System.Console.WriteLine("This introduced member has Tag? False.");
        }
        remove
        {
            global::System.Console.WriteLine("This introduced member has Tag? False.");
        }
    }
}