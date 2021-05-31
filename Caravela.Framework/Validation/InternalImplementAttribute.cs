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
    [AttributeUsage( AttributeTargets.Interface )]
    public class InternalImplementAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildEligibility( IEligibilityBuilder<INamedType> builder )
        {
            builder.MustHaveAccessibility( Accessibility.Public );
        }

        public void BuildAspectClass( IAspectClassBuilder builder ) { }

        public void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.AddReferenceValidator<INamedType, Validator>( builder.TargetDeclaration, new[] { DeclarationReferenceKind.ImplementsInterface } );
        }

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
    }
}