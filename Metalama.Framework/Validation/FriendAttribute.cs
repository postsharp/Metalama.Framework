// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Eligibility;
using System;
using System.Linq;

namespace Metalama.Framework.Validation
{
    /// <summary>
    /// Means that an internal member can be referenced only by a specific type. (Not implemented.)
    /// </summary>
    [AttributeUsage( AttributeTargets.All & ~AttributeTargets.Assembly )]
    public class FriendAttribute : Attribute, IAspect<IMemberOrNamedType>
    {
        private readonly string[] _friendTypes;

        private static readonly DiagnosticDefinition<(IMemberOrNamedType ReferencedDeclaration, INamedType ReferencingType)> _warning =
            new(
                "MY001",
                Severity.Warning,
                "'{0}' cannot be used from '{1}' because of the [Friend] constraint." );

        public FriendAttribute( Type friendType, params Type[] otherFriendTypes )
        {
            this._friendTypes = otherFriendTypes.Append( friendType ).Select( t => t.FullName ).WhereNotNull().ToArray();
        }

        public void BuildEligibility( IEligibilityBuilder<IMemberOrNamedType> builder )
        {
            builder.MustHaveAccessibility( Accessibility.Public, Accessibility.Protected );
        }

        public void BuildAspect( IAspectBuilder<IMemberOrNamedType> builder )
        {
            builder.WithTarget().RegisterReferenceValidator( nameof(this.Validate), ValidatedReferenceKinds.All );
        }

        private void Validate( in ReferenceValidationContext context )
        {
            for ( var type = context.ReferencingType; type != null; type = type.DeclaringType )
            {
                if ( Array.IndexOf( this._friendTypes, type.FullName ) >= 0 )
                {
                    // The reference is allowed.
                    return;
                }
            }

            _warning.WithArguments( ((IMemberOrNamedType) context.ReferencedDeclaration, context.ReferencingType) ).ReportTo( context.Diagnostics );
        }

        public void BuildAspectClass( IAspectClassBuilder builder ) { }
    }
}