// Final Compilation.Emit failed.
// Error CS8854 on `global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ImplicitProperty_AccessorMismatch.IInterface`: `'TargetClass' does not implement interface member 'IInterface.TemplateWithInit.set'. 'TargetClass.TemplateWithInit.init' cannot implement 'IInterface.TemplateWithInit.set'.`
// Error CS8854 on `global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ImplicitProperty_AccessorMismatch.IInterface`: `'TargetClass' does not implement interface member 'IInterface.TemplateWithoutInit.init'. 'TargetClass.TemplateWithoutInit.set' cannot implement 'IInterface.TemplateWithoutInit.init'.`
[Introduction]
public class TargetClass : global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ImplicitProperty_AccessorMismatch.IInterface
{
    public global::System.Int32 TemplateWithInit
    {
        init
        {
        }
    }
    public global::System.Int32 TemplateWithoutInit
    {
        set
        {
        }
    }
}