// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel.References;

internal class SymbolIdRef<T> : DurableRef<T>
    where T : class, ICompilationElement
{
    private SymbolIdRef( string id ) : base( id ) { }

    public SymbolIdRef( in SymbolId id ) : base( id.Id ) { }

    public override SerializableDeclarationId ToSerializableId() => throw new NotSupportedException();

    public override SerializableDeclarationId ToSerializableId( CompilationContext compilationContext )
        => new SymbolId( this.Id ).Resolve( compilationContext.Compilation ).AssertSymbolNotNull().GetSerializableId();

    protected override ISymbol GetSymbol( CompilationContext compilationContext, bool ignoreAssemblyKey = false )
        => new SymbolId( this.Id ).Resolve( compilationContext.Compilation, ignoreAssemblyKey ).AssertSymbolNotNull();

    protected override ICompilationElement? Resolve(
        CompilationModel compilation,
        bool throwIfMissing,
        IGenericContext? genericContext,
        Type interfaceType )
    {
        var symbol = new SymbolId( this.Id ).Resolve( compilation.RoslynCompilation );

        if ( symbol == null )
        {
            return ReturnNullOrThrow( this.Id, throwIfMissing, compilation );
        }

        return ConvertDeclarationOrThrow( compilation.Factory.GetCompilationElement( symbol ).AssertNotNull(), compilation, interfaceType );
    }

    protected override IRef<TOut> CastAsRef<TOut>() => this as IRef<TOut> ?? new SymbolIdRef<TOut>( this.Id );

    public override IFullRef ToFullRef( CompilationContext compilation )
    {
        var symbol = new SymbolId( this.Id ).Resolve( compilation.Compilation )
                     ?? throw new InvalidOperationException( $"Cannot find the symbol '{this.Id}' in '{compilation}'." );

        return compilation.RefFactory.FromAnySymbol( symbol );
    }
}