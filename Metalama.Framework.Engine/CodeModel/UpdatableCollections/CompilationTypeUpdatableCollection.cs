// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Collections;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class CompilationTypeUpdatableCollection : NonUniquelyNamedUpdatableCollection<INamedType>, INamedTypeCollectionImpl
{
    private readonly bool _includeNestedTypes;

    public CompilationTypeUpdatableCollection( CompilationModel compilation, IRef<INamespaceOrNamedType> declaringType, bool includeNestedTypes ) : base(
        compilation,
        declaringType )
    {
        this._includeNestedTypes = includeNestedTypes;
    }

    protected override IEqualityComparer<IRef<INamedType>> MemberRefComparer => this.Compilation.CompilationContext.NamedTypeRefComparer;

    protected override DeclarationKind DeclarationKind => DeclarationKind.NamedType;

    protected override IEnumerable<IRef<INamedType>> GetMemberRefsOfName( string name )
    {
        // TODO: Optimize, what about introduced types?
        if ( this._includeNestedTypes )
        {
            throw new InvalidOperationException( "This method is not supported when the collection recursively includes nested types." );
        }

        return this.Compilation.PartialCompilation.Types
            .Where( t => t.Name == name && this.IsVisible( t ) )
            .Select( s => this.RefFactory.FromSymbol<INamedType>( s ) );
    }

    private bool IsVisible( ISymbol symbol )
    {
        return this.Compilation.Project.ClassificationService?.GetExecutionScope( symbol ) != ExecutionScope.CompileTime;
    }

    protected override IEnumerable<IRef<INamedType>> GetMemberRefs()
    {
        // TODO: Optimize, what about introduced types?
        var topLevelTypes = this.Compilation.PartialCompilation.Types
            .Where( this.IsVisible );

        if ( !this._includeNestedTypes )
        {
            return
                topLevelTypes
                    .Select( s => this.RefFactory.FromSymbol<INamedType>( s ) );
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

#pragma warning disable CS0618 // Type or member is obsolete
            return
                types
                    .Select( s => this.RefFactory.FromSymbol<INamedType>( s ) );
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }

    public IEnumerable<IRef<INamedType>> OfTypeDefinition( INamedType typeDefinition )
    {
        // This does not make sense, since a compilation only has type definitions.
        throw new NotSupportedException();
    }
}