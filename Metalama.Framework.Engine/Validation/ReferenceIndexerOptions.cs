// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Validation;

public sealed class ReferenceIndexerOptions
{
    // Reference kinds that do not require descending into members.

    private const ReferenceKinds _typeDeclarationOnlyKinds = ReferenceKinds.BaseType | ReferenceKinds.Using;

    // Any reference on members (indirectly referencing the type) cannot be detected by identifier filtering.
    private const ReferenceKinds _kindsNotSupportingIdentifierFilteringOnTypes =
        ReferenceKinds.Default | ReferenceKinds.OverrideMember | ReferenceKinds.Assignment
        | ReferenceKinds.Invocation | ReferenceKinds.InterfaceMemberImplementation | ReferenceKinds.NameOf;

    // Reference kinds that do not require descending into implementations. 
    private const ReferenceKinds _memberDeclarationOnlyKinds =
        ReferenceKinds.ParameterType | ReferenceKinds.ReturnType | ReferenceKinds.AttributeType | ReferenceKinds.InterfaceMemberImplementation
        | ReferenceKinds.OverrideMember | ReferenceKinds.MemberType | ReferenceKinds.NameOf | ReferenceKinds.Using;

    private readonly bool _mustDescendIntoMembers;
    private readonly ReferenceKinds _kindsRequiringDescentIntoBaseTypes;
    private readonly bool _mustDescendIntoImplementation;
    private readonly ReferenceKinds _kindsRequiringDescentIntoReferencedDeclaringType;
    private readonly ReferenceKinds _kindsRequiringDescentIntoReferencedNamespace;
    private readonly ReferenceKinds _kindsRequiringDescentIntoReferencedAssembly;
    private readonly ReferenceKinds _kindsSupportingIdentifierFiltering = ReferenceKinds.All;
    private readonly ReferenceKinds _allReferenceKinds;
    private readonly ImmutableHashSet<string> _filteredIdentifiers;

    public ReferenceIndexerOptions( IEnumerable<IReferenceValidatorProperties> validators )
    {
        var filteredIdentifiers = ImmutableHashSet.CreateBuilder<string>();

        foreach ( var validator in validators )
        {
            var validatorReferenceKinds = validator.ReferenceKinds;
            
            this._allReferenceKinds |= validatorReferenceKinds;

            if ( validator is { IncludeDerivedTypes: true, ValidatedDeclarationKind: DeclarationKind.NamedType } )
            {
                this._kindsRequiringDescentIntoBaseTypes |= validatorReferenceKinds;
            }

            if ( (validatorReferenceKinds & ~(_memberDeclarationOnlyKinds | _typeDeclarationOnlyKinds)) != 0 )
            {
                this._mustDescendIntoImplementation = true;
            }

            if ( (validatorReferenceKinds & ~_typeDeclarationOnlyKinds) != 0 )
            {
                this._mustDescendIntoMembers = true;
            }

            var identifierFilteringSupported = false;

            switch ( validator.ValidatedDeclarationKind )
            {
                case DeclarationKind.Namespace:
                    this._kindsRequiringDescentIntoReferencedNamespace |= validatorReferenceKinds;
                    this._kindsSupportingIdentifierFiltering &= ~validatorReferenceKinds;

                    break;

                case DeclarationKind.AssemblyReference:
                case DeclarationKind.Compilation:
                    this._kindsRequiringDescentIntoReferencedAssembly |= validatorReferenceKinds;
                    this._kindsSupportingIdentifierFiltering &= ~validatorReferenceKinds;

                    break;

                case DeclarationKind.NamedType:
                    this._kindsRequiringDescentIntoReferencedDeclaringType |= validatorReferenceKinds;

                    if ( validator.IncludeDerivedTypes )
                    {
                        this._kindsRequiringDescentIntoReferencedAssembly |= validatorReferenceKinds;
                        this._kindsSupportingIdentifierFiltering &= ~validatorReferenceKinds;
                    }
                    else
                    {
                        var kindsNotSupportingIdentifierFiltering = validatorReferenceKinds & _kindsNotSupportingIdentifierFilteringOnTypes;
                        this._kindsSupportingIdentifierFiltering &= ~kindsNotSupportingIdentifierFiltering;
                        identifierFilteringSupported = kindsNotSupportingIdentifierFiltering != 0;
                    }

                    break;

                case DeclarationKind.Constructor:
                case DeclarationKind.Event:
                case DeclarationKind.Method:
                case DeclarationKind.Field:
                case DeclarationKind.Property:
                    identifierFilteringSupported = true;

                    break;
            }

            if ( identifierFilteringSupported )
            {
                var identifier = validator.Identifier;

                if ( identifier != null )
                {
                    filteredIdentifiers.Add( identifier );
                }
            }
        }

        this._filteredIdentifiers = filteredIdentifiers.ToImmutable();
    }

    public ReferenceIndexerOptions( IEnumerable<ReferenceIndexerOptions>? childIndexerOptions )
    {
        this._filteredIdentifiers = ImmutableHashSet<string>.Empty;

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
                this._kindsSupportingIdentifierFiltering &= child._kindsSupportingIdentifierFiltering;
                this._filteredIdentifiers = this._filteredIdentifiers.Union( child._filteredIdentifiers );
            }
        }
    }

    public static ReferenceIndexerOptions Empty { get; } = new( ImmutableArray<IReferenceValidatorProperties>.Empty );

    internal bool MustIndexReferenceKind( ReferenceKinds kind ) => (this._allReferenceKinds & kind) != 0;

    internal bool MustIndexReference( ReferenceKinds kind, in SyntaxToken identifier )
    {
        if ( (this._allReferenceKinds & kind) == 0 )
        {
            return false;
        }

        if ( identifier.RawKind != 0  && (this._kindsSupportingIdentifierFiltering & kind) != 0 )
        {
            var identifierText = identifier.Text;

            if ( identifierText != "var" )
            {
                return this._filteredIdentifiers.Contains( identifierText );
            }
        }

        return true;
    }

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