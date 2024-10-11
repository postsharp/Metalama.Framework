// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Introductions.BuilderData;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.References;

/// <summary>
/// An implementation of <see cref="IRef"/> based on <see cref="IDeclarationBuilder"/>.
/// </summary>
internal sealed partial class IntroducedRef<T> : FullRef<T>, IIntroducedRef
    where T : class, IDeclaration
{
    private readonly GenericContext _genericContext; // Gives the type arguments for the builder.

    public DeclarationBuilderData BuilderData { get; }

    public IFullRef? ReplacedDeclaration
        => this.BuilderData switch
        {
            ConstructorBuilderData { ReplacedImplicitConstructor: { } replacedImplicitConstructor } => replacedImplicitConstructor,
            PropertyBuilderData { OriginalField: { } originalField } => originalField,
            _ => null
        };

    public IntroducedRef( DeclarationBuilderData builder, RefFactory refFactory, GenericContext? genericContext = null ) : base( refFactory )
    {
        // Type parameter must match the builder type.
        Invariant.Assert(
            builder.DeclarationKind.GetPossibleDeclarationInterfaceTypes().Contains( typeof(T) ),
            $"The interface type was expected to be of type {builder.DeclarationKind.GetPossibleDeclarationInterfaceTypes()} but was {typeof(T)}." );

        // Constructor replacements must be resolved upstream, but this invariant can no longer be enforced here because the reference
        // is built when the BuilderData is being built.

        // References to promoted fields must be a SymbolRef to the IFieldSymbol if it is an IRef<IField>.
        Invariant.Assert( !(typeof(T) == typeof(IField) && builder is PropertyBuilderData) );

        this.BuilderData = builder;
        this._genericContext = genericContext ?? GenericContext.Empty;
    }

    public override bool IsDefinition => this._genericContext.IsEmptyOrIdentity;

    public override IFullRef<T> DefinitionRef => this.IsDefinition ? this : (IFullRef<T>) this.BuilderData.ToFullRef();

    public override FullRef<T> WithGenericContext( GenericContext genericContext )
        => genericContext.IsEmptyOrIdentity ? this : new IntroducedRef<T>( this.BuilderData, this.RefFactory, genericContext );

    public override IFullRef? ContainingDeclaration => this.BuilderData.ContainingDeclaration;

    public override IFullRef<INamedType> DeclaringType => this.BuilderData.DeclaringType.AssertNotNull();

    public override string? Name
        => this.BuilderData switch
        {
            NamedDeclarationBuilderData named => named.Name,
            _ => null
        };

    public override SerializableDeclarationId ToSerializableId() => this.ConstructedDeclaration.ToSerializableId();

    protected override ISymbol GetSymbolIgnoringRefKind( CompilationContext compilationContext, bool ignoreAssemblyKey = false )
        => throw new NotSupportedException();

    public override ISymbol GetClosestContainingSymbol()
    {
        for ( var ancestor = (IFullRef) this.BuilderData.ContainingDeclaration; ancestor != null; ancestor = ancestor.ContainingDeclaration )
        {
            if ( ancestor is ISymbolRef symbolBasedDeclaration )
            {
                return symbolBasedDeclaration.Symbol;
            }
        }

        // We should always have an containing symbol.
        throw new AssertionFailedException();
    }

    public override SyntaxTree? PrimarySyntaxTree => this.BuilderData.PrimarySyntaxTree;

    private GenericContext SelectGenericContext( IGenericContext? genericContext )
    {
        if ( this._genericContext.IsEmptyOrIdentity )
        {
            return (GenericContext?) genericContext ?? GenericContext.Empty;
        }
        else if ( genericContext is null or { IsEmptyOrIdentity: true } )
        {
            return this._genericContext;
        }
        else
        {
            throw new InvalidOperationException( "Cannot combine two non-empty generic contexts." );
        }
    }

    protected override ICompilationElement? Resolve(
        CompilationModel compilation,
        bool throwIfMissing,
        IGenericContext? genericContext,
        Type interfaceType )
        => ConvertDeclarationOrThrow(
            compilation.Factory.GetDeclaration( this.BuilderData, this.SelectGenericContext( genericContext ), interfaceType ),
            compilation,
            interfaceType );

    public override string ToString() => this.BuilderData.ToString()!;

    protected override IFullRef<TOut> CastAsFullRef<TOut>()
    {
        if ( this is IFullRef<TOut> desired )
        {
            return desired;
        }
        else if ( this.BuilderData.DeclarationKind == DeclarationKind.Property && typeof(TOut) == typeof(IField) )
        {
            var redirectedField = ((PropertyBuilderData) this.BuilderData).OriginalField;

            if ( redirectedField != null )
            {
                return (IFullRef<TOut>) redirectedField.WithGenericContext( this._genericContext );
            }
        }
        else if ( this.BuilderData.DeclarationKind == DeclarationKind.Field && typeof(TOut) == typeof(IProperty) )
        {
            var overridingProperty = ((FieldBuilderData) this.BuilderData).OverridingProperty;

            if ( overridingProperty != null )
            {
                return (IFullRef<TOut>) overridingProperty.WithGenericContext( this._genericContext );
            }
        }

        throw new InvalidCastException( $"Cannot convert the IRef<{typeof(T).Name}> to IRef<{typeof(TOut).Name}>) for '{this}'." );
    }

    public override bool Equals( IRef? other, RefComparison comparison )
    {
        // NOTE: By convention, we want references to be considered different if they resolve to different targets. Therefore, for promoted fields,
        // an IRef<IField> or an IRef<IProperty> to the same PromotedField will be considered different.
        // Since all references are canonical, we only need to support comparison of references of the same type.
        // A reference of any other type is not equal.

        if ( other is not IntroducedRef<T> otherRef )
        {
            return false;
        }

        Invariant.Assert(
            this.CompilationContext == otherRef.CompilationContext ||
            comparison is RefComparison.Structural or RefComparison.StructuralIncludeNullability,
            "Compilation mistmatch in a non-structural comparison." );

        if ( !ReferenceEquals( this.BuilderData, otherRef.BuilderData ) )
        {
            return false;
        }

        if ( !this._genericContext.Equals( otherRef._genericContext ) )
        {
            return false;
        }

        return true;
    }

    public override int GetHashCode( RefComparison comparison ) => HashCode.Combine( this.BuilderData.GetHashCode(), this._genericContext );

    public override DeclarationKind DeclarationKind => this.BuilderData.DeclarationKind;
}