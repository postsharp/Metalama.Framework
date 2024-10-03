// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.References;

internal sealed class SymbolRef<T> : CompilationBoundRef<T>, ISymbolRef
    where T : class, ICompilationElement
{
    public ISymbol Symbol { get; }

    public override CompilationContext CompilationContext { get; }

    public override bool IsDefinition => this.Symbol.IsDefinitionSafe();

    [Memo]
    public override IRef Definition => new SymbolRef<T>( this.Symbol.OriginalDefinition, this.CompilationContext, this.TargetKind );

    public override RefTargetKind TargetKind { get; }

    public override string Name => this.Symbol.Name;

    public override SerializableDeclarationId ToSerializableId() => this.Symbol.GetSerializableId();

    public SymbolRef( ISymbol symbol, CompilationContext compilationContext, RefTargetKind targetKind = RefTargetKind.Default )
    {
        Invariant.Assert(
            symbol.GetDeclarationKind( compilationContext ).GetPossibleDeclarationInterfaceTypes( targetKind ).Contains( typeof(T) ),
            $"The interface type was expected to be of type {symbol.GetDeclarationKind( compilationContext ).GetPossibleDeclarationInterfaceTypes( targetKind )} but was {typeof(T)}." );

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

        this.Symbol = symbol;
        this.TargetKind = targetKind;
        this.CompilationContext = compilationContext;
    }

    public override ICompilationBoundRefImpl WithGenericContext( GenericContext genericContext )
    {
        if ( genericContext.IsEmptyOrIdentity )
        {
            return this;
        }
        else
        {
            // TODO: Test.
            var mappedSymbol =
                genericContext.NamedTypeSymbol!.GetMembers( this.Symbol.Name ).Single( s => s.OriginalDefinition.Equals( this.Symbol.OriginalDefinition ) );

            return new SymbolRef<T>( mappedSymbol, this.CompilationContext, this.TargetKind );
        }
    }

    public override IRefStrategy Strategy => this.CompilationContext.SymbolRefStrategy;

    protected override ISymbol GetSymbolIgnoringRefKind( CompilationContext compilationContext, bool ignoreAssemblyKey = false )
        => compilationContext.SymbolTranslator.Translate( this.Symbol ).AssertSymbolNotNull();

    protected override T? Resolve(
        CompilationModel compilation,
        bool throwIfMissing,
        IGenericContext? genericContext )
    {
        var translatedSymbol = compilation.CompilationContext.SymbolTranslator.Translate( this.Symbol, this.CompilationContext.Compilation );

        if ( translatedSymbol == null )
        {
            return ReturnNullOrThrow( MetalamaStringFormatter.Instance.Format( this.Symbol ), throwIfMissing, compilation );
        }

        return ConvertDeclarationOrThrow(
            compilation.Factory.GetCompilationElement( translatedSymbol, this.TargetKind, genericContext ).AssertNotNull(),
            compilation );
    }

    public override string ToString()
        => this.TargetKind switch
        {
            RefTargetKind.Default => this.Symbol.ToDisplayString( SymbolDisplayFormat.CSharpShortErrorMessageFormat ),
            _ => $"{this.Symbol.ToDisplayString( SymbolDisplayFormat.CSharpShortErrorMessageFormat )}:{this.TargetKind}"
        };

    public override IRefImpl<TOut> As<TOut>()
        => (IRefImpl<TOut>) (object) this; // There should be no reason to upcast since we always create instances of the right type.

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