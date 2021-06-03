[TestOutput]
[Introduction]
public class TargetClass
: global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ImplicitMembers.IInterface
{


    public global::System.Int32 InterfaceMethod()
    {
        global::System.Console.WriteLine("This is introduced interface method.");
        return default(global::System.Int32);
    }

    public event global::System.EventHandler Event; global::System.Int32 global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ImplicitMembers.IInterface.InterfaceMethod()
    {
        return this.InterfaceMethod();
    }

    event global::System.EventHandler global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ImplicitMembers.IInterface.Event
    {
        add
        {
            this.Event += value;
        }

        remove
        {
            this.Event -= value;
        }
    }
}