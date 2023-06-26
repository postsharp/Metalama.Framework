// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class CompilationTypeUpdatableCollection : NonUniquelyNamedUpdatableCollection<INamedType>, INamedTypeCollectionImpl
{
    private readonly bool _includeNestedTypes;

    public CompilationTypeUpdatableCollection( CompilationModel compilation, INamespaceOrTypeSymbol declaringType, bool includeNestedTypes ) : base(
        compilation,
        declaringType )
    {
        this._includeNestedTypes = includeNestedTypes;
    }

    protected override IEqualityComparer<MemberRef<INamedType>> MemberRefComparer => this.Compilation.CompilationContext.NamedTypeRefComparer;

    protected override IEnumerable<ISymbol> GetSymbolsOfName( string name )
    {
        if ( this._includeNestedTypes )
        {
            throw new InvalidOperationException( "This method is not supported when the collection recursively includes nested types." );
        }

        return this.Compilation.PartialCompilation.Types
            .Where( t => t.Name == name && this.IsVisible( t ) );
    }

    protected override IEnumerable<ISymbol> GetSymbols()
    {
        var topLevelTypes = this.Compilation.PartialCompilation.Types
            .Where( this.IsVisible );

        if ( !this._includeNestedTypes )
        {
            return topLevelTypes;
        }
        else
        {
            var types = new List<ISymbol>();

            void ProcessType( INamedTypeSymbol type )
            {
                types.Add( type );

                foreach ( var nestedType in type.GetTypeMembers() )
                {
                    ProcessType( nestedType );
                }
            }

            foreach ( var type in topLevelTypes )
            {
                ProcessType( type );
            }

            return types;
        }
    }

    public ImmutableArray<MemberRef<INamedType>> OfTypeDefinition( INamedType typeDefinition )
    {
        var comparer = (DeclarationEqualityComparer) this.Compilation.Comparers.GetTypeComparer( TypeComparison.Default );

        return
            this.GetSymbols()
                .Where( t => comparer.Is( (ITypeSymbol)t, typeDefinition.GetSymbol(), ConversionKind.IgnoreTypeArguments ) )
                .Where( this.IsSymbolIncluded )
                .Select( x => new MemberRef<INamedType>( x, this.Compilation.CompilationContext ) )
                .ToImmutableArray();
    }
}