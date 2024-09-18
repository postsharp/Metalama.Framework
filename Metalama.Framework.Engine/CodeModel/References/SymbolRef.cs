// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Comparers;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System.Collections;

namespace Metalama.Framework.Engine.CodeModel.References;

internal sealed class SymbolRef<T> : BaseRef<T>, ISymbolRef
    where T : class, ICompilationElement
{
    public ISymbol Symbol { get; }

    private protected override CompilationContext CompilationContext { get; }

    public override DeclarationRefTargetKind TargetKind { get; }

    public override string Name => this.Symbol.Name;

    public override SerializableDeclarationId ToSerializableId() => this.Symbol.GetSerializableId();

    public SymbolRef( ISymbol symbol, CompilationContext compilationContext, DeclarationRefTargetKind targetKind = DeclarationRefTargetKind.Default )
    {
        this.Symbol = symbol;
        this.TargetKind = targetKind;
        this.CompilationContext = compilationContext;
    }

    public override IRefStrategy Strategy => this.CompilationContext.SymbolRefStrategy;

    protected override ISymbol GetSymbolIgnoringKind( CompilationContext compilationContext, bool ignoreAssemblyKey = false )
    {
        return compilationContext.SymbolTranslator.Translate( this.Symbol ).AssertSymbolNotNull();
    }

    protected override T? Resolve( CompilationModel compilation, ReferenceResolutionOptions options, bool throwIfMissing, IGenericContext? genericContext )
    {
        var translatedSymbol = compilation.CompilationContext.SymbolTranslator.Translate( this.Symbol, compilation.RoslynCompilation );

        if ( translatedSymbol == null )
        {
            return ReturnNullOrThrow( MetalamaStringFormatter.Instance.Format( this.Symbol ), throwIfMissing, compilation );
        }

        return ConvertOrThrow( compilation.Factory.GetCompilationElement( translatedSymbol, this.TargetKind ).AssertNotNull(), compilation );
    }

    public override bool Equals( IRef? other )
        => other?.Unwrap() is ISymbolRef symbolRef && StructuralComparisons.StructuralEqualityComparer.Equals( symbolRef.Symbol, this.Symbol )
                                                   && this.TargetKind == symbolRef.TargetKind;

    protected override int GetHashCodeCore() => StructuralSymbolComparer.Default.GetHashCode( this.Symbol );

    public override string ToString()
        => this.TargetKind switch
        {
            DeclarationRefTargetKind.Default => this.Symbol.ToString()!,
            _ => $"{this.Symbol}:{this.TargetKind}"
        };
}