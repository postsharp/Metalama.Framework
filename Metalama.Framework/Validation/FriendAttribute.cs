// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Validation
{
    /// <summary>
    /// Means that an internal member can be referenced only by a specific type. (Not implemented.)
    /// </summary>
    [AttributeUsage( AttributeTargets.All & ~AttributeTargets.Assembly )]
    [Obsolete( "Not implemented." )]
    public class FriendAttribute : Attribute, IAspect<IMemberOrNamedType>
    {
        private readonly Type[] _friendTypes;

        public FriendAttribute( Type friendType, params Type[] otherFriendTypes )
        {
            this._friendTypes = otherFriendTypes.Append( friendType ).ToArray();
        }

        public void BuildEligibility( IEligibilityBuilder<IMemberOrNamedType> builder )
        {
            builder.MustHaveAccessibility( Accessibility.Public, Accessibility.Protected );
        }

        public void BuildAspect( IAspectBuilder<IMemberOrNamedType> builder )
        {
            var properties = new Dictionary<string, string> { ["FriendTypes"] = string.Join( ";", this._friendTypes.Select( t => t.FullName ) ) };

            builder.AddReferenceValidator<IMemberOrNamedType, Validator>( builder.Target, new[] { DeclarationReferenceKind.Any }, properties );
        }

        private class Validator : IDeclarationReferenceValidator<IMemberOrNamedType>
        {
            private string[] _friendTypes = null!;

            public void Initialize( IReadOnlyDictionary<string, string> properties )
            {
                this._friendTypes = properties["FriendTypes"].Split( ';' );
            }

            public void ValidateReference( in ValidateReferenceContext<IMemberOrNamedType> reference )
            {
                for ( var type = reference.ReferencingType; type != null; type = type.DeclaringType )
                {
                    if ( Array.IndexOf( this._friendTypes, type.FullName ) >= 0 )
                    {
                        // The reference is allowed.
                        return;
                    }
                }

                // TODO: Report
                //
                // reference.Diagnostics.Report(
                //     reference.DiagnosticLocation,
                //     null!,
                //     reference.ReferencedDeclaration,
                //     reference.ReferencedDeclaration,
                //     this._friendTypes );
            }
        }

        public void BuildAspectClass( IAspectClassBuilder builder ) { }
    }
}