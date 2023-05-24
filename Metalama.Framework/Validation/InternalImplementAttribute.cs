// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
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
        public override void BuildEligibility( IEligibilityBuilder<INamedType> builder ) => builder.MustHaveAccessibility( Accessibility.Public );

        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Outbound.ValidateReferences( Validate, ReferenceKinds.BaseType );
        }

        private static void Validate( in ReferenceValidationContext context )
        {
            if ( context.ReferencingDeclaration.Compilation != context.ReferencedDeclaration.Compilation )
            {
                context.Diagnostics.Report( FrameworkDiagnosticDescriptors.InternalImplementConstraint.WithArguments( (context.ReferencedDeclaration, context.ReferencingType) ) );
            }
        }
    }
}