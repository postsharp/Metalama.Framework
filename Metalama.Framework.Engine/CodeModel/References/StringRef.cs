// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel.References;

internal class StringRef<T> : BaseRef<T>, IStringRef
    where T : class, ICompilationElement
{
    public string Id { get; }

    private protected override CompilationContext CompilationContext { get; }

    public StringRef( string id, CompilationContext compilationContext )
    {
        this.Id = id;
        this.CompilationContext = compilationContext;
    }

    public override string Name => throw new NotSupportedException();

    public override SerializableDeclarationId ToSerializableId()
    {
        if ( IsDeclarationId( this.Id ) && this.TargetKind == DeclarationRefTargetKind.Default )
        {
            return new SerializableDeclarationId( this.Id );
        }
        else
        {
            return base.ToSerializableId();
        }
    }

    private static bool IsDeclarationId( string id ) => char.IsLetter( id[0] ) && id[1] == ':' && id[0] != SerializableTypeIdResolverForSymbol.Prefix[0];

    private static bool IsTypeId( string id )
        => id.StartsWith( SerializableTypeIdResolverForSymbol.LegacyPrefix, StringComparison.Ordinal )
           || id.StartsWith( SerializableTypeIdResolverForSymbol.Prefix, StringComparison.Ordinal );

    protected override ISymbol GetSymbolIgnoringKind( CompilationContext compilationContext, bool ignoreAssemblyKey = false )
    {
        ISymbol? symbol;

        if ( IsDeclarationId( this.Id ) )
        {
            symbol = new SerializableDeclarationId( this.Id ).ResolveToSymbolOrNull( compilationContext );
        }
        else if ( IsTypeId( this.Id ) )
        {
            symbol = compilationContext.SerializableTypeIdResolver.ResolveId( new SerializableTypeId( this.Id ) );
        }
        else
        {
            var symbolKey = new SymbolId( this.Id );

            symbol = symbolKey.Resolve( compilationContext.Compilation, ignoreAssemblyKey );
        }

        if ( symbol == null )
        {
            throw new SymbolNotFoundException( this.Id, compilationContext.Compilation );
        }

        return symbol;
    }

    protected override T? Resolve( CompilationModel compilation, ReferenceResolutionOptions options, bool throwIfMissing, IGenericContext? genericContext )
    {
        if ( IsDeclarationId( this.Id ) )
        {
            var declaration = new SerializableDeclarationId( this.Id ).ResolveToDeclaration( compilation );

            if ( declaration == null )
            {
                return this.ReturnNullOrThrow( this.Id, throwIfMissing, compilation );
            }

            return this.ConvertOrThrow( declaration, compilation );
        }
        else if ( IsTypeId( this.Id ) )
        {
            try
            {
                var type = new SerializableTypeId( this.Id ).Resolve( compilation );

                return this.ConvertOrThrow( type, compilation );
            }
            catch ( InvalidOperationException ex )
            {
                return this.ReturnNullOrThrow( this.Id, throwIfMissing, compilation, ex );
            }
        }
        else
        {
            var symbol = new SymbolId( this.Id ).Resolve( compilation.RoslynCompilation );

            if ( symbol == null )
            {
                return this.ReturnNullOrThrow( this.Id, throwIfMissing, compilation );
            }

            return this.ConvertOrThrow( compilation.Factory.GetCompilationElement( symbol ).AssertNotNull(), compilation );
        }
    }

    public override bool Equals( IRef? other ) => other?.Unwrap() is IStringRef stringRef && stringRef.Id == this.Id;

    protected override int GetHashCodeCore() => this.Id.GetHashCode();

    public override string ToString() => this.Id;
}