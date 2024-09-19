// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.References;

internal sealed class SymbolRef<T> : CompilationBoundRef<T>, ISymbolRef
    where T : class, ICompilationElement
{
    public ISymbol Symbol { get; }

    public override CompilationContext CompilationContext { get; }

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

    protected override ISymbol GetSymbolIgnoringKind( bool ignoreAssemblyKey = false )
    {
        return this.CompilationContext.SymbolTranslator.Translate( this.Symbol ).AssertSymbolNotNull();
    }

    protected override T? Resolve( CompilationModel compilation, ReferenceResolutionOptions options, bool throwIfMissing, IGenericContext? genericContext )
    {
        Invariant.Assert( compilation.GetCompilationContext() == this.CompilationContext, "CompilationContext mismatch." );

        var translatedSymbol = compilation.CompilationContext.SymbolTranslator.Translate( this.Symbol, compilation.RoslynCompilation );

        if ( translatedSymbol == null )
        {
            return ReturnNullOrThrow( MetalamaStringFormatter.Instance.Format( this.Symbol ), throwIfMissing, compilation );
        }

        return ConvertOrThrow( compilation.Factory.GetCompilationElement( translatedSymbol, this.TargetKind ).AssertNotNull(), compilation );
    }

    public override bool Equals( IRef? other )
    {
        if ( other is not ISymbolRef symbolRef )
        {
            return false;
        }

        Invariant.Assert( this.CompilationContext == symbolRef.CompilationContext, "Attempted to compare two symbols of different compilations." );

        // Intentionally using non-structural comparison because we are in the same compilation.
        return SymbolEqualityComparer.Default.Equals( symbolRef.Symbol, this.Symbol )
               && this.TargetKind == symbolRef.TargetKind;
    }

    protected override int GetHashCodeCore() => SymbolEqualityComparer.Default.GetHashCode( this.Symbol );

    public override string ToString()
        => this.TargetKind switch
        {
            RefTargetKind.Default => this.Symbol.ToString()!,
            _ => $"{this.Symbol}:{this.TargetKind}"
        };

    public override IRefImpl<TOut> As<TOut>()
        => (IRefImpl<TOut>) (object) this; // There should be no reason to upcast since we always create instances of the right type.
}