[Introduction]
internal class TargetClass { 

    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Events.CopyAttributeToIntroducedEvent.FooAttribute]
    public event global::System.EventHandler? Event
    {
        [return: global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Events.CopyAttributeToIntroducedEvent.FooAttribute]
        [param: global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Events.CopyAttributeToIntroducedEvent.FooAttribute]
        add
        {
            global::System.Console.WriteLine("Original add accessor.");
        }

        [return: global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Events.CopyAttributeToIntroducedEvent.FooAttribute]
        [param: global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Events.CopyAttributeToIntroducedEvent.FooAttribute]
        remove
        {
            global::System.Console.WriteLine("Original remove accessor.");
        }
    }

    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Events.CopyAttributeToIntroducedEvent.FooAttribute]
    public event global::System.EventHandler? FieldLikeEvent;}