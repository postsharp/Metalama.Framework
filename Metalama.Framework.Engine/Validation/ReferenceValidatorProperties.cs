// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Engine.Validation;

internal class ReferenceValidatorProperties : IReferenceValidatorProperties
{
    internal ReferenceValidatorProperties(
        IDeclaration validatedDeclaration,
        ReferenceKinds referenceKinds,
        bool includeDerivedTypes )
    {
        this.ValidatedDeclarationKind = validatedDeclaration.DeclarationKind;

        this.ValidatedIdentifier = validatedDeclaration switch
        {
            IConstructor constructor => constructor.DeclaringType.Name,
            INamedDeclaration namedDeclaration => namedDeclaration.Name,
            _ => null
        };

        if ( referenceKinds.IsDefined( ReferenceKinds.BaseType ) && validatedDeclaration is INamedType { IsSealed: true } )
        {
            referenceKinds &= ~ReferenceKinds.BaseType;
        }

        this.ReferenceKinds = referenceKinds & GetReferenceKindsSupportedByDeclarationKind( this.ValidatedDeclarationKind );

        if ( includeDerivedTypes )
        {
            this.IncludeDerivedTypes = validatedDeclaration switch
            {
                INamedType namedType => !namedType.IsSealed,
                INamespace or ICompilation => true,
                _ => this.IncludeDerivedTypes
            };
        }
    }

    public ReferenceKinds ReferenceKinds { get; }

    public bool IncludeDerivedTypes { get; }

    public DeclarationKind ValidatedDeclarationKind { get; }

    /// <summary>
    /// Gets the identifier on which the reference to be validated can be filtered.
    /// </summary>
    public string? ValidatedIdentifier { get; }

    private static ReferenceKinds GetReferenceKindsSupportedByDeclarationKind( DeclarationKind declarationKind )
        => declarationKind switch
        {
            DeclarationKind.Compilation or DeclarationKind.Namespace or DeclarationKind.NamedType or DeclarationKind.AssemblyReference => ReferenceKinds.All,
            DeclarationKind.Constructor => ReferenceKinds.BaseConstructor | ReferenceKinds.ObjectCreation,
            DeclarationKind.Event or DeclarationKind.Method => ReferenceKinds.Default | ReferenceKinds.Invocation | ReferenceKinds.NameOf
                                                               | ReferenceKinds.InterfaceMemberImplementation | ReferenceKinds.OverrideMember
                                                               | ReferenceKinds.Assignment,
            DeclarationKind.Property => ReferenceKinds.Default | ReferenceKinds.Assignment | ReferenceKinds.NameOf
                                        | ReferenceKinds.InterfaceMemberImplementation | ReferenceKinds.OverrideMember,
            DeclarationKind.Field => ReferenceKinds.Default | ReferenceKinds.Assignment | ReferenceKinds.NameOf,
            DeclarationKind.Finalizer => ReferenceKinds.None,
            DeclarationKind.Indexer => ReferenceKinds.Default | ReferenceKinds.Assignment | ReferenceKinds.InterfaceMemberImplementation
                                       | ReferenceKinds.OverrideMember,
            DeclarationKind.Operator => ReferenceKinds.Invocation,
            _ => ReferenceKinds.None
        };
}