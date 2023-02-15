// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class TypeUpdatableCollection : NonUniquelyNamedUpdatableCollection<INamedType>
{
    public TypeUpdatableCollection( CompilationModel compilation, INamespaceOrTypeSymbol declaringType ) : base( compilation, declaringType ) { }

    protected override bool IsSymbolIncluded( ISymbol symbol )
    {
        if ( !base.IsSymbolIncluded( symbol ) )
        {
            return false;
        }

        if ( symbol.ContainingAssembly == this.Compilation.RoslynCompilation.Assembly )
        {
            // For types defined in the current assembly, we need to take partial compilations into account.

            return IsIncludedInPartialCompilation( (INamedTypeSymbol) symbol );

            bool IsIncludedInPartialCompilation( INamedTypeSymbol t )
                => t switch
                {
                    { ContainingType: { } containingType } => IsIncludedInPartialCompilation( containingType ),
                    _ => this.Compilation.PartialCompilation.Types.Contains( t.OriginalDefinition )
                };
        }
        else
        {
            return true;
        }
    }

    protected override IEqualityComparer<MemberRef<INamedType>> MemberRefComparer => this.Compilation.CompilationContext.NamedTypeRefComparer;

    protected override IEnumerable<ISymbol> GetSymbols( string name )
        => this.DeclaringTypeOrNamespace.GetTypeMembers( name )
            .Where( this.IsSymbolIncluded );

    protected override IEnumerable<ISymbol> GetSymbols()
        => this.DeclaringTypeOrNamespace.GetTypeMembers()
            .Where( this.IsSymbolIncluded );
}