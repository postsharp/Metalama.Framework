[Introduction]
public class TargetClass : IBaseInterface, global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.IInterface
{
    public void BaseMethod()
    {
    }
    public int BaseProperty { get; set; }
    public event EventHandler? BaseEvent;
    public void Witness()
    {
        global::System.Console.WriteLine("InterfaceType: IInterface, Action: Implement");
        global::System.Console.WriteLine("InterfaceType: IBaseInterface, Action: Ignore");
        global::System.Console.WriteLine("Member: IInterface.Method(), Action: Introduce, Target: TargetClass.Method");
        global::System.Console.WriteLine("Member: Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.IInterface.Property, Action: Introduce, Target: TargetClass.Property");
        global::System.Console.WriteLine("Member: IInterface.Event, Action: Introduce, Target: TargetClass.Event");
        global::System.Console.WriteLine("Member: IBaseInterface.BaseMethod(), Action: UseExisting, Target: TargetClass.BaseMethod()");
        global::System.Console.WriteLine("Member: Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.IBaseInterface.BaseProperty, Action: UseExisting, Target: Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.AdviceResult_Ignore_ImplementedBase.TargetClass.BaseProperty");
        global::System.Console.WriteLine("Member: IBaseInterface.BaseEvent, Action: UseExisting, Target: TargetClass.BaseEvent");
    }
    public global::System.Int32 Property { get; set; }
    public void Method()
    {
    }
    public event global::System.EventHandler? Event;
}
