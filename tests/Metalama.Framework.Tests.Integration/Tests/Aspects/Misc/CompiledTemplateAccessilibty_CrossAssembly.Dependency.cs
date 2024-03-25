using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.CompiledTemplateAccessilibty_CrossAssembly;

public class MyAspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        builder.Advice.IntroduceMethod(builder.Target, nameof(Private), args: new { x = new PrivateCompileTimeType() });
        builder.Advice.IntroduceMethod(builder.Target, nameof(Protected), args: new { x = new ProtectedCompileTimeType() });
        builder.Advice.IntroduceMethod(builder.Target, nameof(Internal), args: new { x = new InternalCompileTimeType() });
        builder.Advice.IntroduceMethod(builder.Target, nameof(Public), args: new { x = new PublicCompileTimeType() });
        builder.Advice.IntroduceMethod(builder.Target, nameof(PrivateProtected), args: new { x = new ProtectedCompileTimeType(), y = new InternalCompileTimeType() });
        builder.Advice.IntroduceMethod(builder.Target, nameof(ProtectedInternal), args: new { x = new ProtectedInternalCompileTimeType() });
    }

    [Template]
    private string Private(PrivateCompileTimeType x) => $"{x}";

    // TODO: call Tostring everywhere else

    [Template]
    protected string Protected(ProtectedCompileTimeType x) => $"{x}";

    [Template]
    internal string Internal(InternalCompileTimeType x) => $"{x}";

    [Template]
    public string Public(PublicCompileTimeType x) => $"{x}";

    [Template]
    private protected string PrivateProtected(ProtectedCompileTimeType x, InternalCompileTimeType y) => $"{x}, {y}";

    [Template]
    protected internal string ProtectedInternal(ProtectedInternalCompileTimeType x) => $"{x}";

    [CompileTime]
    private class PrivateCompileTimeType { }

    [CompileTime]
    protected class ProtectedCompileTimeType { }

    [CompileTime]
    protected internal class ProtectedInternalCompileTimeType { }
}

[CompileTime]
internal class InternalCompileTimeType { }

[CompileTime]
public class PublicCompileTimeType { }