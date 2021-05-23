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
    /// Means that an internal member can be referenced only by a specific type.
    /// </summary>
    [AttributeUsage( AttributeTargets.All & ~AttributeTargets.Assembly )]
    public class FriendAttribute : Attribute, IAspect<IMember>
    {
        private readonly Type[] _friendTypes;

        public FriendAttribute( Type friendType, params Type[] otherFriendTypes )
        {
            this._friendTypes = otherFriendTypes.Append( friendType ).ToArray();
        }

        public void BuildEligibility( IEligibilityBuilder<IMember> builder )
        {
            builder.MustHaveAccessibility( Accessibility.Public, Accessibility.Protected );
        }

        public void BuildAspect( IAspectBuilder<IMember> builder )
        {
            var properties = new Dictionary<string, string> { ["FriendTypes"] = string.Join( ";", this._friendTypes.Select( t => t.FullName ) ) };

            builder.AddReferenceValidator<IMember, Validator>( builder.TargetDeclaration, new[] { DeclarationReferenceKind.Any }, properties );
        }

        private class Validator : IDeclarationReferenceValidator<IMember>
        {
            private string[] _friendTypes = null!;

            public void Initialize( IReadOnlyDictionary<string, string> properties )
            {
                this._friendTypes = properties["FriendTypes"].Split( ';' );
            }

            public void ValidateReference( in ValidateReferenceContext<IMember> reference )
            {
                for ( var type = reference.ReferencingType; type != null; type = type.DeclaringType )
                {
                    if ( Array.IndexOf( this._friendTypes, type.FullName ) >= 0 )
                    {
                        // The reference is allowed.
                        return;
                    }
                }

                reference.Diagnostics.Report(
                    reference.DiagnosticLocation,
                    null!,
                    reference.ReferencedDeclaration,
                    reference.ReferencedDeclaration,
                    this._friendTypes );
            }
        }
    }
}