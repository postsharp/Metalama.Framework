// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Validation;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Validation;

public sealed class ReferenceIndexerOptions
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
    private readonly ReferenceKinds _allReferenceKinds;

    public ReferenceIndexerOptions( IEnumerable<IReferenceValidatorProperties> validators )
    {
        foreach ( var validator in validators )
        {
            this._allReferenceKinds |= validator.ReferenceKinds;

            if ( validator is { IncludeDerivedTypes: true, ValidatedDeclarationKind: DeclarationKind.NamedType } )
            {
                this._kindsRequiringDescentIntoBaseTypes |= validator.ReferenceKinds;
            }

            if ( (validator.ReferenceKinds & ~(_memberDeclarationOnlyKinds | _typeDeclarationOnlyKinds)) != 0 )
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

    public ReferenceIndexerOptions( IEnumerable<ReferenceIndexerOptions>? childIndexerOptions )
    {
        if ( childIndexerOptions != null )
        {
            foreach ( var child in childIndexerOptions )
            {
                this._allReferenceKinds |= child._allReferenceKinds;

                this._mustDescendIntoImplementation |= child._mustDescendIntoImplementation;
                this._mustDescendIntoMembers |= child._mustDescendIntoMembers;
                this._kindsRequiringDescentIntoReferencedAssembly |= child._kindsRequiringDescentIntoReferencedAssembly;
                this._kindsRequiringDescentIntoReferencedNamespace |= child._kindsRequiringDescentIntoReferencedNamespace;
                this._kindsRequiringDescentIntoReferencedDeclaringType |= child._kindsRequiringDescentIntoReferencedDeclaringType;
                this._kindsRequiringDescentIntoBaseTypes |= child._kindsRequiringDescentIntoBaseTypes;
            }
        }
    }

    public static ReferenceIndexerOptions Empty { get; } = new( ImmutableArray<IReferenceValidatorProperties>.Empty );

    public bool MustReferenceKind( ReferenceKinds kind ) => (this._allReferenceKinds & kind) != 0;

    public bool MustDescendIntoMembers() => this._mustDescendIntoMembers;

    public bool MustDescendIntoImplementation() => this._mustDescendIntoImplementation;

    public bool MustDescendIntoReferencedBaseTypes( ReferenceKinds referenceKinds ) => (referenceKinds & this._kindsRequiringDescentIntoBaseTypes) != 0;

    public bool MustDescendIntoReferencedDeclaringType( ReferenceKinds referenceKinds )
        => (referenceKinds & this._kindsRequiringDescentIntoReferencedDeclaringType) != 0;

    public bool MustDescendIntoReferencedNamespace( ReferenceKinds referenceKinds )
        => (referenceKinds & this._kindsRequiringDescentIntoReferencedNamespace) != 0;

    public bool MustDescendIntoReferencedAssembly( ReferenceKinds referenceKinds ) => (referenceKinds & this._kindsRequiringDescentIntoReferencedAssembly) != 0;
}