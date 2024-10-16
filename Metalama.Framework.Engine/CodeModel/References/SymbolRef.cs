// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.GenericContexts;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.SerializableIds;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.References;

internal sealed partial class SymbolRef<T> : FullRef<T>, ISymbolRef<T>
    where T : class, ICompilationElement
{
    private readonly GenericContext? _genericContextForSymbolMapping;

    public ISymbol Symbol { get; }

    public override bool IsDefinition => this.Symbol.IsDefinitionSafe() && this._genericContextForSymbolMapping == null;

    [Memo]
    public override IFullRef<T> DefinitionRef => new SymbolRef<T>( this.Symbol.OriginalDefinition, null, this.RefFactory, this.TargetKind );

    public override RefTargetKind TargetKind { get; }

    [Memo]
    public override IFullRef ContainingDeclaration => this.RefFactory.FromAnySymbol( this.Symbol.ContainingSymbol );

    [Memo]
    public override IFullRef<INamedType> DeclaringType => this.RefFactory.FromSymbol<INamedType>( this.Symbol.ContainingType );

    public override string Name => this.Symbol.Name;

    public override SerializableDeclarationId ToSerializableId() => this.Symbol.GetSerializableId();

    public SymbolRef(
        ISymbol symbol,
        GenericContext? genericContextForSymbolMapping,
        RefFactory refFactory,
        RefTargetKind targetKind = RefTargetKind.Default ) : base( refFactory )
    {
        Invariant.Assert(
            symbol.GetDeclarationKind( refFactory.CompilationContext ).GetPossibleDeclarationInterfaceTypes( targetKind ).Contains( typeof(T) ),
            $"The interface type was expected to be of type {symbol.GetDeclarationKind( refFactory.CompilationContext ).GetPossibleDeclarationInterfaceTypes( targetKind )} but was {typeof(T)}." );

        // Verify that RefTargetKind is used only in reference to declarations that don't have a symbol, i.e. the reference must be normalized
        // before calling the constructor.
        Invariant.Assert(
            targetKind == RefTargetKind.Default ||
            (targetKind == RefTargetKind.Return && symbol.Kind == SymbolKind.Method) ||
            (targetKind is RefTargetKind.PropertyGet or RefTargetKind.PropertyGetReturnParameter
             && symbol is { Kind: SymbolKind.Field } or IPropertySymbol { GetMethod: null }) ||
            (targetKind is RefTargetKind.PropertySet or RefTargetKind.PropertySetParameter or RefTargetKind.PropertySetReturnParameter
             && symbol is { Kind: SymbolKind.Field } or IPropertySymbol { SetMethod: null }) ||
            (targetKind is RefTargetKind.EventRaise or RefTargetKind.EventRaiseParameter or RefTargetKind.EventRaiseReturnParameter &&
             symbol.Kind == SymbolKind.Event),
            $"Invalid RefTargetKind.{targetKind} for {symbol.Kind}." );

        // If a genericContextForSymbolMapping is supplied, we must have a symbol definition.
        Invariant.Assert( genericContextForSymbolMapping == null || symbol.IsDefinitionSafe() );

        this.Symbol = symbol;
        this.TargetKind = targetKind;
        this._genericContextForSymbolMapping = genericContextForSymbolMapping;
    }

    public override FullRef<T> WithGenericContext( GenericContext genericContext )
    {
        if ( !this.IsDefinition )
        {
            throw new InvalidOperationException(
                $"{nameof(this.WithGenericContext)} must be called on a generic definition, but the current object is a generic instance." );
        }

        switch ( genericContext.Kind )
        {
            case GenericContextKind.Null:
                return this;

            case GenericContextKind.Symbol:

                Invariant.Assert( this._genericContextForSymbolMapping == null );

                var mappedSymbol =
                    ((SymbolGenericContext) genericContext).NamedTypeSymbol!.GetMembers( this.Symbol.Name )
                    .Single( s => s.OriginalDefinition.Equals( this.Symbol.OriginalDefinition ) );

                return this.RefFactory.FromSymbol<T>( mappedSymbol, targetKind: this.TargetKind );

            case GenericContextKind.Introduced:
                return this.RefFactory.FromSymbol<T>( this.Symbol, genericContext, targetKind: this.TargetKind );

            default:
                throw new AssertionFailedException();
        }
    }

    protected override ISymbol GetSymbolIgnoringRefKind( CompilationContext compilationContext, bool ignoreAssemblyKey = false )
        => compilationContext.SymbolTranslator.Translate( this.Symbol ).AssertSymbolNotNull();

    public override SyntaxTree? PrimarySyntaxTree => this.Symbol.GetClosestPrimaryDeclarationSyntax()?.SyntaxTree;

    protected override ICompilationElement? Resolve(
        CompilationModel compilation,
        bool throwIfMissing,
        IGenericContext? genericContext,
        Type interfaceType )
    {
        var translatedSymbol = compilation.CompilationContext.SymbolTranslator.Translate( this.Symbol, this.CompilationContext.Compilation );

        if ( translatedSymbol == null )
        {
            return ReturnNullOrThrow( MetalamaStringFormatter.Instance.Format( this.Symbol ), throwIfMissing, compilation );
        }

        return ConvertDeclarationOrThrow(
            compilation.Factory.GetCompilationElement( translatedSymbol, this.TargetKind, genericContext, interfaceType ).AssertNotNull(),
            compilation,
            interfaceType );
    }

    public override string ToString()
        => this.TargetKind switch
        {
            RefTargetKind.Default => this.Symbol.ToDebugString(),
            _ => $"{this.Symbol.ToDebugString()}:{this.TargetKind}"
        };

    protected override IFullRef<TOut> CastAsFullRef<TOut>() => (IFullRef<TOut>) (object) this;

    public override int GetHashCode( RefComparison comparison )
        => HashCode.Combine( comparison.GetSymbolComparer().GetHashCode( this.Symbol ), this.TargetKind );

    public override DeclarationKind DeclarationKind => this.TargetKind.ToDeclarationKind() ?? this.Symbol.GetDeclarationKind( this.CompilationContext );

    public override bool Equals( IRef? other, RefComparison comparison )
    {
        // NOTE: By convention, we want references to be considered different if they resolve to different targets. Therefore, for promoted fields,
        // an IRef<IField> or an IRef<IProperty> to the same PromotedField will be considered different.
        // Since all references are canonical, we only need to support comparison of references of the same type.
        // A reference of any other type is not equal.

        if ( other is not SymbolRef<T> otherRef )
        {
            return false;
        }

        Invariant.Assert(
            this.CompilationContext == otherRef.CompilationContext ||
            comparison is RefComparison.Structural or RefComparison.StructuralIncludeNullability,
            "Compilation mistmatch in a non-structural comparison." );

        return comparison.GetSymbolComparer( this.CompilationContext, otherRef.CompilationContext ).Equals( this.Symbol, otherRef.Symbol )
               && this.TargetKind == otherRef.TargetKind;
    }
}