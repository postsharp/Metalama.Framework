[Introduction]
public class TargetClass : global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Tags_ExplicitInterface.IInterface1, global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Tags_ExplicitInterface.IInterface2
{
    global::System.Int32 global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Tags_ExplicitInterface.IInterface1.Property1
    {
        get
        {
            global::System.Console.WriteLine("This is introduced interface member with Tag TestValue_For_Interface1.");
            return (global::System.Int32)42;
        }
        set
        {
            global::System.Console.WriteLine("This is introduced interface member with Tag TestValue_For_Interface1.");
        }
    }
    global::System.Int32 global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Tags_ExplicitInterface.IInterface2.Property2
    {
        get
        {
            global::System.Console.WriteLine("This is introduced interface member with Tag TestValue_For_Interface2.");
            return (global::System.Int32)42;
        }
        set
        {
            global::System.Console.WriteLine("This is introduced interface member with Tag TestValue_For_Interface2.");
        }
    }
    global::System.Int32 global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Tags_ExplicitInterface.IInterface1.InterfaceMethod1()
    {
        global::System.Console.WriteLine("This is introduced interface member with Tag TestValue_For_Interface1.");
        return default(global::System.Int32);
    }
    global::System.Int32 global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Tags_ExplicitInterface.IInterface2.InterfaceMethod2()
    {
        global::System.Console.WriteLine("This is introduced interface member with Tag TestValue_For_Interface2.");
        return default(global::System.Int32);
    }
    event global::System.EventHandler global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Tags_ExplicitInterface.IInterface1.Event1
    {
        add
        {
            global::System.Console.WriteLine("This is introduced interface member with Tag TestValue_For_Interface1.");
        }
        remove
        {
            global::System.Console.WriteLine("This is introduced interface member with Tag TestValue_For_Interface1.");
        }
    }
    event global::System.EventHandler global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Tags_ExplicitInterface.IInterface2.Event2
    {
        add
        {
            global::System.Console.WriteLine("This is introduced interface member with Tag TestValue_For_Interface2.");
        }
        remove
        {
            global::System.Console.WriteLine("This is introduced interface member with Tag TestValue_For_Interface2.");
        }
    }
}