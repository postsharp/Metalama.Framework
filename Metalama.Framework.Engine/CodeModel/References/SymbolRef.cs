// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.References;

internal sealed class SymbolRef<T> : CompilationBoundRef<T>, ISymbolRef
    where T : class, ICompilationElement
{
    public ISymbol Symbol { get; }

    public override CompilationContext CompilationContext { get; }

    public override bool IsDefinition => this.Symbol.IsDefinition;

    [Memo]
    public override IRef Definition => new SymbolRef<T>( this.Symbol.OriginalDefinition, this.CompilationContext, this.TargetKind );

    public override RefTargetKind TargetKind { get; }

    public override string Name => this.Symbol.Name;

    public override SerializableDeclarationId ToSerializableId() => this.Symbol.GetSerializableId();

    public SymbolRef( ISymbol symbol, CompilationContext compilationContext, RefTargetKind targetKind = RefTargetKind.Default )
    {
        Invariant.Assert(
            symbol.GetDeclarationKind( compilationContext ).GetRefInterfaceType( targetKind ) == typeof(T)
            || (typeof(T) == typeof(ICompilation) && symbol.Kind == SymbolKind.Assembly),
            $"Interface type mismatch: expected {symbol.GetDeclarationKind( compilationContext ).GetRefInterfaceType().Name} but got {typeof(T).Name}." );

        this.Symbol = symbol;
        this.TargetKind = targetKind;
        this.CompilationContext = compilationContext;
    }

    public override IRefStrategy Strategy => this.CompilationContext.SymbolRefStrategy;

    protected override ISymbol GetSymbolIgnoringRefKind( CompilationContext compilationContext, bool ignoreAssemblyKey = false )
        => compilationContext.SymbolTranslator.Translate( this.Symbol ).AssertSymbolNotNull();

    protected override T? Resolve(
        CompilationModel compilation,
        ReferenceResolutionOptions options,
        bool throwIfMissing,
        IGenericContext? genericContext )
    {
        var translatedSymbol = compilation.CompilationContext.SymbolTranslator.Translate( this.Symbol, this.CompilationContext.Compilation );

        if ( translatedSymbol == null )
        {
            return ReturnNullOrThrow( MetalamaStringFormatter.Instance.Format( this.Symbol ), throwIfMissing, compilation );
        }

        return ConvertOrThrow(
            compilation.Factory.GetCompilationElement( translatedSymbol, this.TargetKind, genericContext ).AssertNotNull(),
            compilation );
    }

    protected override bool EqualsCore( IRef? other, RefComparison comparison, IEqualityComparer<ISymbol> symbolComparer )
    {
        if ( other is not SymbolRef<T> symbolRef )
        {
            return false;
        }

        return this.TargetKind == symbolRef.TargetKind
               && symbolComparer.Equals( symbolRef.Symbol, this.Symbol );
    }

    protected override int GetHashCodeCore( RefComparison comparison, IEqualityComparer<ISymbol> symbolComparer ) => symbolComparer.GetHashCode( this.Symbol );

    public override string ToString()
        => this.TargetKind switch
        {
            RefTargetKind.Default => this.Symbol.ToDisplayString( SymbolDisplayFormat.CSharpShortErrorMessageFormat ),
            _ => $"{this.Symbol.ToDisplayString( SymbolDisplayFormat.CSharpShortErrorMessageFormat )}:{this.TargetKind}"
        };

    public override IRefImpl<TOut> As<TOut>()
        => (IRefImpl<TOut>) (object) this; // There should be no reason to upcast since we always create instances of the right type.
}