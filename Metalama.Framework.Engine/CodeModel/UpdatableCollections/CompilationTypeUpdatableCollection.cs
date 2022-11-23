// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CompileTime;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal class CompilationTypeUpdatableCollection : NonUniquelyNamedUpdatableCollection<INamedType>
{
    private readonly bool _includeNestedTypes;

    public CompilationTypeUpdatableCollection( CompilationModel compilation, INamespaceOrTypeSymbol declaringType, bool includeNestedTypes ) : base(
        compilation,
        declaringType )
    {
        this._includeNestedTypes = includeNestedTypes;
    }

    protected override IEnumerable<ISymbol> GetSymbols( string name )
    {
        if ( this._includeNestedTypes )
        {
            throw new InvalidOperationException( "This method is not supported when the collection recursively includes nested types." );
        }

        return this.Compilation.PartialCompilation.Types
            .Where(
                t => t.Name == name && this.Compilation.CompilationServices.SymbolClassifier.GetTemplatingScope( t ).GetExpressionExecutionScope()
                    != TemplatingScope.CompileTimeOnly );
    }

    protected override IEnumerable<ISymbol> GetSymbols()
    {
        var topLevelTypes = this.Compilation.PartialCompilation.Types
            .Where( t => this.Compilation.CompilationServices.SymbolClassifier.GetTemplatingScope( t ).GetExpressionExecutionScope() != TemplatingScope.CompileTimeOnly );

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
}