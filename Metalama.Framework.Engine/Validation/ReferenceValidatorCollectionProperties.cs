// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Validation;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Validation;

public sealed class ReferenceValidatorCollectionProperties
{
    // Reference kinds that do not require descending into members.

    private const ReferenceKinds _typeDeclarationOnlyKinds = ReferenceKinds.BaseType | ReferenceKinds.Using;

    // Reference kinds that do not require descending into implementations. 
    private const ReferenceKinds _memberDeclarationOnlyKinds =
        ReferenceKinds.ParameterType | ReferenceKinds.ReturnType | ReferenceKinds.AttributeType | ReferenceKinds.InterfaceMemberImplementation
        | ReferenceKinds.OverrideMember | ReferenceKinds.MemberType;

    private readonly bool _mustDescendIntoMembers;
    private readonly ReferenceKinds _kindsRequiringDescentIntoBaseTypes;
    private readonly bool _mustDescendIntoImplementation;
    private readonly ReferenceKinds _kindsRequiringDescentIntoReferencedDeclaringType;
    private readonly ReferenceKinds _kindsRequiringDescentIntoReferencedNamespace;
    private readonly ReferenceKinds _kindsRequiringDescentIntoReferencedAssembly;

    public ReferenceValidatorCollectionProperties( IEnumerable<IReferenceValidatorProperties> validators )
    {
        foreach ( var validator in validators )
        {
            if ( validator is { IncludeDerivedTypes: true, ValidatedDeclarationKind: DeclarationKind.NamedType } )
            {
                this._kindsRequiringDescentIntoBaseTypes |= validator.ReferenceKinds;
            }

            if ( (validator.ReferenceKinds & ~_memberDeclarationOnlyKinds) != 0 )
            {
                this._mustDescendIntoImplementation = true;
            }

            if ( (validator.ReferenceKinds & ~_typeDeclarationOnlyKinds) != 0 )
            {
                this._mustDescendIntoMembers = true;
            }

            switch ( validator.ValidatedDeclarationKind )
            {
                case DeclarationKind.Namespace:
                    this._kindsRequiringDescentIntoReferencedNamespace |= validator.ReferenceKinds;

                    break;

                case DeclarationKind.AssemblyReference:
                case DeclarationKind.Compilation:
                    this._kindsRequiringDescentIntoReferencedAssembly |= validator.ReferenceKinds;

                    break;

                case DeclarationKind.NamedType:
                    this._kindsRequiringDescentIntoReferencedDeclaringType |= validator.ReferenceKinds;

                    break;
            }
        }
    }

    public ReferenceValidatorCollectionProperties( IEnumerable<ReferenceValidatorCollectionProperties>? childCollectionProperties )
    {
        if ( childCollectionProperties != null )
        {
            foreach ( var childCollectionProperty in childCollectionProperties )
            {
                this._mustDescendIntoImplementation |= childCollectionProperty._mustDescendIntoImplementation;
                this._mustDescendIntoMembers |= childCollectionProperty._mustDescendIntoMembers;
                this._kindsRequiringDescentIntoReferencedAssembly |= childCollectionProperty._kindsRequiringDescentIntoReferencedAssembly;
                this._kindsRequiringDescentIntoReferencedNamespace |= childCollectionProperty._kindsRequiringDescentIntoReferencedNamespace;
                this._kindsRequiringDescentIntoReferencedDeclaringType |= childCollectionProperty._kindsRequiringDescentIntoReferencedDeclaringType;
                this._kindsRequiringDescentIntoBaseTypes |= childCollectionProperty._kindsRequiringDescentIntoBaseTypes;
            }
        }
    }

    public static ReferenceValidatorCollectionProperties Empty { get; } = new( ImmutableArray<IReferenceValidatorProperties>.Empty );

    internal bool MustDescendIntoMembers() => this._mustDescendIntoMembers;

    internal bool MustDescendIntoImplementation() => this._mustDescendIntoImplementation;

    internal bool MustDescendIntoReferencedBaseTypes( ReferenceKinds referenceKinds ) => (referenceKinds & this._kindsRequiringDescentIntoBaseTypes) != 0;

    internal bool MustDescendIntoReferencedDeclaringType( ReferenceKinds referenceKinds )
        => (referenceKinds & this._kindsRequiringDescentIntoReferencedDeclaringType) != 0;

    internal bool MustDescendIntoReferencedNamespace( ReferenceKinds referenceKinds )
        => (referenceKinds & this._kindsRequiringDescentIntoReferencedNamespace) != 0;

    internal bool MustDescendIntoReferencedAssembly( ReferenceKinds referenceKinds )
        => (referenceKinds & this._kindsRequiringDescentIntoReferencedAssembly) != 0;
}