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

    public override CompilationContext CompilationContext { get; }

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

    protected override ISymbol GetSymbolIgnoringKind( bool ignoreAssemblyKey = false )
    {
        ISymbol? symbol;

        if ( IsDeclarationId( this.Id ) )
        {
            symbol = new SerializableDeclarationId( this.Id ).ResolveToSymbolOrNull( this.CompilationContext );
        }
        else if ( IsTypeId( this.Id ) )
        {
            symbol = this.CompilationContext.SerializableTypeIdResolver.ResolveId( new SerializableTypeId( this.Id ) );
        }
        else
        {
            var symbolKey = new SymbolId( this.Id );

            symbol = symbolKey.Resolve( this.CompilationContext.Compilation, ignoreAssemblyKey );
        }

        if ( symbol == null )
        {
            throw new SymbolNotFoundException( this.Id, this.CompilationContext.Compilation );
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

    public override bool Equals( IRef? other )
    {
        if ( other?.Unwrap() is not IStringRef stringRef )
        {
            return false;
        }

        Invariant.Assert( this.CompilationContext == stringRef.CompilationContext, "Attempted to compare two symbols of different compilations." );

        return stringRef.Id == this.Id;
    }

    protected override int GetHashCodeCore()
    {
#if NET5_0_OR_GREATER
        return this.Id.GetHashCode( StringComparison.Ordinal );
#else
        return this.Id.GetHashCode();
#endif
    }

    public override string ToString() => this.Id;
}