// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using System;

namespace Metalama.Framework.Validation
{
    /// <summary>
    /// A custom attribute that means that the interface cannot be implemented by another assembly than
    /// the one that declared it, except if the referencing assembly sees the internals of the declaring assembly.
    /// (The enforcement of this attribute is not implemented.) 
    /// </summary>
    [AttributeUsage( AttributeTargets.Interface )]
    public class InternalImplementAttribute : TypeAspect
    {
        public override void BuildEligibility( IEligibilityBuilder<INamedType> builder )
            =>

                // Coverage: Ignore
                builder.MustHaveAccessibility( Accessibility.Public );

        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            /*
            builder.AddReferenceValidator<INamedType, Validator>( builder.Target, new[] { DeclarationReferenceKind.ImplementsInterface } );
            */
        }

        /*
        private class Validator : IDeclarationReferenceValidator<INamedType>
        {
            public void Initialize( IReadOnlyDictionary<string, string> properties ) { }

            public void ValidateReference( in ValidateReferenceContext<INamedType> reference )
            {
                if ( reference.ReferencingDeclaration.Compilation != reference.ReferencedDeclaration.Compilation )
                {
                    reference.Diagnostics.Report( reference.DiagnosticLocation, null!, reference.ReferencingDeclaration, reference.ReferencedDeclaration );
                }
            }
        }
        
        */

        public override void BuildAspectClass( IAspectClassBuilder builder ) { }
    }
}