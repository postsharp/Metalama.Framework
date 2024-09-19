// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel.References;

internal sealed class StringRef<T> : BaseRef<T>, IStringRef
    where T : class, ICompilationElement
{
    public string Id { get; }

    public StringRef( string id )
    {
        this.Id = id;
    }

    public override string Name => throw new NotSupportedException();

    public override SerializableDeclarationId ToSerializableId()
    {
        if ( IsDeclarationId( this.Id ) )
        {
            return new SerializableDeclarationId( this.Id );
        }
        else
        {
            throw new InvalidOperationException( "The reference represents a non-named type." );
        }
    }

    public override IRef<T> ToPortable() => this;

    public override bool IsPortable => true;

    private static bool IsDeclarationId( string id )
    {
        return (char.IsLetter( id[0] ) && id[1] == ':' && id[0] != SerializableTypeIdResolverForSymbol.Prefix[0])
               || id.StartsWith( "Assembly:", StringComparison.Ordinal );
    }

    private static bool IsTypeId( string id )
        => id.StartsWith( SerializableTypeIdResolverForSymbol.LegacyPrefix, StringComparison.Ordinal )
           || id.StartsWith( SerializableTypeIdResolverForSymbol.Prefix, StringComparison.Ordinal );

    protected override ISymbol GetSymbol( CompilationContext compilationContext, bool ignoreAssemblyKey = false )
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

    public override ISymbol GetClosestContainingSymbol( CompilationContext compilationContext )
    {
        // TODO: Handle references to builders.
        return this.GetSymbol( compilationContext );
    }

    protected override T? Resolve( CompilationModel compilation, ReferenceResolutionOptions options, bool throwIfMissing, IGenericContext? genericContext )
    {
        if ( IsDeclarationId( this.Id ) )
        {
            var declaration = new SerializableDeclarationId( this.Id ).ResolveToDeclaration( compilation );

            if ( declaration == null )
            {
                return ReturnNullOrThrow( this.Id, throwIfMissing, compilation );
            }

            return ConvertOrThrow( declaration, compilation );
        }
        else if ( IsTypeId( this.Id ) )
        {
            try
            {
                var type = new SerializableTypeId( this.Id ).Resolve( compilation );

                return ConvertOrThrow( type, compilation );
            }
            catch ( InvalidOperationException ex )
            {
                return ReturnNullOrThrow( this.Id, throwIfMissing, compilation, ex );
            }
        }
        else
        {
            var symbol = new SymbolId( this.Id ).Resolve( compilation.RoslynCompilation );

            if ( symbol == null )
            {
                return ReturnNullOrThrow( this.Id, throwIfMissing, compilation );
            }

            return ConvertOrThrow( compilation.Factory.GetCompilationElement( symbol ).AssertNotNull(), compilation );
        }
    }

    public override bool Equals( IRef? other, RefComparisonOptions comparisonOptions )
    {
        // String comparisons are always portable and null-sensitive, so we ignore all flags.

        if ( other is not IStringRef stringRef )
        {
            return false;
        }

        return stringRef.Id == this.Id;
    }

    public override int GetHashCode( RefComparisonOptions comparisonOptions )
    {
#if NET5_0_OR_GREATER
        return this.Id.GetHashCode( StringComparison.Ordinal );
#else
        return this.Id.GetHashCode();
#endif
    }

    public override string ToString() => this.Id;

    public override IRefImpl<TOut> As<TOut>() => this as IRefImpl<TOut> ?? new StringRef<TOut>( this.Id );
}