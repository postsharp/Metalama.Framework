using Caravela.Framework.Project;

namespace Caravela.Framework.Code
{
    [CompileTime]
    public enum MethodKind
    {
        Default,

        Constructor,
        StaticConstructor,
        Finalizer,

        PropertyGet,
        PropertySet,

        EventAdd,
        EventRemove,
        EventRaise,

        // DelegateInvoke
        // FunctionPointerSignature

        ExplicitInterfaceImplementation,

        ConversionOperator,
        UserDefinedOperator,

        LocalFunction
    }
}