#if TEST_OPTIONS
// @DesignTime
# endif

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Validation;
using System;

namespace Metalama.Framework.IntegrationTests.Aspects.InvalidCode.TransitiveValidator_SerializableId;

public class TestValidateAttribute : PropertyAspect
{
    private static readonly DiagnosticDefinition<(ReferenceKinds ReferenceKinds, IDeclaration Declaration, string SyntaxKind)> _warning =
        new("MY001", Severity.Warning, "Reference constraint of type '{0}' in declaration '{1}' (SyntaxKind={2}).");

    public override void BuildAspect(IAspectBuilder<IProperty> builder)
    {
        builder.Outbound.ValidateInboundReferences(Validate, Validation.ReferenceGranularity.ParameterOrAttribute, Validation.ReferenceKinds.All, ReferenceValidationOptions.IncludeDerivedTypes);
    }

    private static void Validate(ReferenceValidationContext context)
    {
        context.Diagnostics.Report(x => _warning.WithArguments((x.ReferenceKind, x.OriginDeclaration, x.Source.Kind)));
    }
}

public class BaseReferencedClass<T>
{
    [TestValidate]
    public virtual T Foo { get => default; set { } }
}

public class DerivedReferencedClass : BaseReferencedClass<int>
{
    public override int Foo { get => 42; set { } }
}