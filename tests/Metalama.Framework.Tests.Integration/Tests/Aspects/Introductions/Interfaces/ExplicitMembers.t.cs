[Introduction]
    public class TargetClass:global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers.IInterface{ 

global::System.Int32 global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers.IInterface.InterfaceMethod()
{
    global::System.Console.WriteLine("This is introduced interface member.");
        return default(global::System.Int32);
}

global::System.Int32 global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers.IInterface.Property
{
    get
    {
                global::System.Console.WriteLine("This is introduced interface member.");
                return default(global::System.Int32);
    


    }

    set
    {
                global::System.Console.WriteLine("This is introduced interface member.");
                }
}

global::System.Int32 global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers.IInterface.AutoProperty { get; set; }

event global::System.EventHandler global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers.IInterface.Event
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
private global::System.EventHandler? _eventField;

event global::System.EventHandler? global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers.IInterface.EventField
{
    add
    {
                                    this._eventField += value;
                        }

    remove
    {
                                    this._eventField -= value;
                        }
}}