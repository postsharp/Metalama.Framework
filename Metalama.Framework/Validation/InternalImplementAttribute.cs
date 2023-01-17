// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Eligibility;
using System;

namespace Metalama.Framework.Validation
{
    /// <summary>
    /// A custom attribute that means that the interface cannot be implemented by another assembly than
    /// the one that declared it, except if the referencing assembly sees the internals of the declaring assembly.
    /// </summary>
    [AttributeUsage( AttributeTargets.Interface )]
    internal sealed class InternalImplementAttribute : TypeAspect
    {
        private static readonly DiagnosticDefinition<(IDeclaration InterfaceType, INamedType ImplementingType)> _warning =
            new DiagnosticDefinition<(IDeclaration ReferencedType, INamedType ReferencingType)>(
                "MY001",
                Severity.Warning,
                "The interface '{0}' cannot be implemented by the type '{1}' because of the [InternalImplement] constraint." );

        public override void BuildEligibility( IEligibilityBuilder<INamedType> builder ) => builder.MustHaveAccessibility( Accessibility.Public );

        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Amend.ValidateReferences( Validate, ReferenceKinds.BaseType );
        }

        private static void Validate( in ReferenceValidationContext context )
        {
            if ( context.ReferencingDeclaration.Compilation != context.ReferencedDeclaration.Compilation )
            {
                context.Diagnostics.Report( _warning.WithArguments( (context.ReferencedDeclaration, context.ReferencingType) ) );
            }
        }
    }
}