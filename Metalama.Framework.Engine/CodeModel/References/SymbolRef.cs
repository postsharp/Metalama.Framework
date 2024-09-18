﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Comparers;
using Microsoft.CodeAnalysis;
using System;
using System.Collections;

namespace Metalama.Framework.Engine.CodeModel.References;

internal class SymbolRef<T> : BaseRef<T>, ISymbolRef
    where T : class, ICompilationElement
{
    public ISymbol Symbol { get; }

    private protected override CompilationContext? CompilationContext { get; }

    public override DeclarationRefTargetKind TargetKind { get; }

    public override string Name => this.Symbol.Name;

    public override SerializableDeclarationId ToSerializableId() => throw new NotImplementedException();

    public SymbolRef( ISymbol symbol, CompilationContext compilationContext, DeclarationRefTargetKind targetKind = DeclarationRefTargetKind.Default )
    {
        this.Symbol = symbol;
        this.TargetKind = targetKind;
        this.CompilationContext = compilationContext;
    }

    protected override ISymbol GetSymbolIgnoringKind( CompilationContext compilationContext, bool ignoreAssemblyKey = false )
    {
        return compilationContext.SymbolTranslator.Translate( this.Symbol ).AssertSymbolNotNull();
    }

    protected override T? Resolve( CompilationModel compilation, ReferenceResolutionOptions options, bool throwIfMissing, IGenericContext? genericContext )
    {
        var translatedSymbol = compilation.CompilationContext.SymbolTranslator.Translate( this.Symbol, compilation.RoslynCompilation );

        if ( translatedSymbol == null )
        {
            return this.ReturnNullOrThrow( MetalamaStringFormatter.Instance.Format( this.Symbol ), throwIfMissing, compilation );
        }

        return this.ConvertOrThrow( compilation.Factory.GetCompilationElement( translatedSymbol, this.TargetKind ).AssertNotNull(), compilation );
    }

    public override bool Equals( IRef? other )
        => other?.Unwrap() is ISymbolRef symbolRef && StructuralComparisons.StructuralEqualityComparer.Equals( symbolRef.Symbol, this.Symbol )
                                                   && this.TargetKind == symbolRef.TargetKind;

    protected override int GetHashCodeCore() => StructuralSymbolComparer.Default.GetHashCode( this.Symbol );

    public override string ToString() => this.Symbol.ToString();
}