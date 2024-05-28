// Final Compilation.Emit failed.
// Error CS0737 on `global::Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Interfaces.Accessibility.IInterface`: `'TargetClass' does not implement interface member 'IInterface.AutoProperty'. 'TargetClass.AutoProperty' cannot implement an interface member because it is not public.`
// Error CS0277 on `global::Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Interfaces.Accessibility.IInterface`: `'TargetClass' does not implement interface member 'IInterface.AutoProperty_PrivateSetter.set'. 'TargetClass.AutoProperty_PrivateSetter.set' is not public.`
// Error CS0737 on `global::Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Interfaces.Accessibility.IInterface`: `'TargetClass' does not implement interface member 'IInterface.Event'. 'TargetClass.Event' cannot implement an interface member because it is not public.`
// Error CS0737 on `global::Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Interfaces.Accessibility.IInterface`: `'TargetClass' does not implement interface member 'IInterface.EventField'. 'TargetClass.EventField' cannot implement an interface member because it is not public.`
// Error CS0737 on `global::Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Interfaces.Accessibility.IInterface`: `'TargetClass' does not implement interface member 'IInterface.Method()'. 'TargetClass.Method()' cannot implement an interface member because it is not public.`
// Error CS0737 on `global::Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Interfaces.Accessibility.IInterface`: `'TargetClass' does not implement interface member 'IInterface.Property'. 'TargetClass.Property' cannot implement an interface member because it is not public.`
// Error CS0737 on `global::Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Interfaces.Accessibility.IInterface`: `'TargetClass' does not implement interface member 'IInterface.Property_ExpressionBody'. 'TargetClass.Property_ExpressionBody' cannot implement an interface member because it is not public.`
// Error CS0737 on `global::Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Interfaces.Accessibility.IInterface`: `'TargetClass' does not implement interface member 'IInterface.Property_GetOnly'. 'TargetClass.Property_GetOnly' cannot implement an interface member because it is not public.`
[Introduction]
public class TargetClass : global::Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Interfaces.Accessibility.IInterface
{
    private global::System.Int32 AutoProperty { get; set; }
    public global::System.Int32 AutoProperty_PrivateSetter { get; private set; }
    private global::System.Int32 Property
    {
        get
        {
            return (global::System.Int32)42;
        }
        set
        {
        }
    }
    private global::System.Int32 Property_ExpressionBody
    {
        get
        {
            return (global::System.Int32)42;
        }
    }
    private global::System.Int32 Property_GetOnly
    {
        get
        {
            return (global::System.Int32)42;
        }
    }
    public global::System.Int32 Property_PrivateSetter
    {
        get
        {
            return (global::System.Int32)42;
        }
        private set
        {
        }
    }
    private void Method()
    {
        global::System.Console.WriteLine("Introduced interface member");
    }
    private event global::System.EventHandler? Event
    {
        add
        {
        }
        remove
        {
        }
    }
    private event global::System.EventHandler? EventField;
}