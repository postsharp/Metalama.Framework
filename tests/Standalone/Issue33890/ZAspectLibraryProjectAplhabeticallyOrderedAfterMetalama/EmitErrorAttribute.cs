using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace AspectLibraryProject
{
    public class EmitErrorAttribute : OverrideMethodAspect
    {
        private static DiagnosticDefinition<INamedType> _error = new(
            "MY001",
            Severity.Error,
            "My error.");

        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            builder.Diagnostics.Report(_error.WithArguments(null));
        }

        public override dynamic OverrideMethod()
        {
            throw new NotImplementedException();
        }
    }
}