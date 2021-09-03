// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using System;
using System.Collections.Generic;

#pragma warning disable 618 // Not implemented

namespace Caravela.Framework.Validation
{
    /// <summary>
    /// A custom attribute that means that the interface cannot be implemented by another assembly than
    /// the one that declared it, except if the referencing assembly sees the internals of the declaring assembly.
    /// (The enforcement of this attribute is not implemented.) 
    /// </summary>
    [AttributeUsage( AttributeTargets.Interface )]
    public class InternalImplementAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildEligibility( IEligibilityBuilder<INamedType> builder )
        {
            // Coverage: Ignore
            builder.MustHaveAccessibility( Accessibility.Public );
        }

        public void BuildAspect( IAspectBuilder<INamedType> builder )
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

        public void BuildAspectClass( IAspectClassBuilder builder ) { }
        
    }
}