// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class NamespaceUpdatableCollection : UniquelyNamedUpdatableCollection<INamespace>
{
    public NamespaceUpdatableCollection( CompilationModel compilation, Ref<INamespace> declaringNamespace ) : base(
        compilation,
        declaringNamespace.As<INamespaceOrNamedType>() ) { }

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
            {
                return t switch
                {
                    { ContainingType: { } containingType } => IsIncludedInPartialCompilation( containingType ),
                    _ => this.Compilation.PartialCompilation.Types.Contains( t.OriginalDefinition )
                };
            }
        }
        else
        {
            return true;
        }
    }

    protected override IEqualityComparer<MemberRef<INamespace>> MemberRefComparer => this.Compilation.CompilationContext.NamespaceRefComparer;

    protected override MemberRef<INamespace> GetMemberRef( string name )
        => this.DeclaringTypeOrNamespace.Target switch
        {
            INamespaceSymbol symbol =>
                symbol.TranslateIfNecessary( this.Compilation.CompilationContext )
                    .GetNamespaceMembers()
                    .Where( n => n.Name == name )
                    .Where( this.IsSymbolIncluded )
                    .Select( s => new MemberRef<INamespace>( s, this.Compilation.CompilationContext ) )
                    .FirstOrDefault(),
            INamespace =>
                default,
            _ => throw new AssertionFailedException( $"Unsupported {this.DeclaringTypeOrNamespace.Target}" )
        };

    protected override IEnumerable<MemberRef<INamespace>> GetMemberRefs()
        => this.DeclaringTypeOrNamespace.Target switch
        {
            INamespaceSymbol symbol =>
                symbol.TranslateIfNecessary( this.Compilation.CompilationContext )
                    .GetNamespaceMembers()
                    .Where( this.IsSymbolIncluded )
                    .Select( s => new MemberRef<INamespace>( s, this.Compilation.CompilationContext ) )
                    .ToImmutableArray(),
            INamespace =>
                ImmutableArray<MemberRef<INamespace>>.Empty,
            _ => throw new AssertionFailedException( $"Unsupported {this.DeclaringTypeOrNamespace.Target}" )
        };
}