// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.DesignTime
{
    internal partial class PartialCompilationModel : CompilationModel
    {
        private readonly IEnumerable<ITypeSymbol> _types;

        protected PartialCompilationModel( CSharpCompilation roslynCompilation, IEnumerable<ITypeSymbol> types ) : base( roslynCompilation )
        {
            this._types = types;
        }

        protected override IEnumerable<ITypeSymbol> GetTypes() => this._types;

        public static PartialCompilationModel CreateInitialInstance( SemanticModel semanticModel )
        {
            FindTypesVisitor findTypesVisitor = new();
            findTypesVisitor.Visit( semanticModel.SyntaxTree.GetRoot() );

            HashSet<ITypeSymbol> types = new();

            void AddTypeRecursive( ITypeSymbol type )
            {
                if ( type.ContainingAssembly != semanticModel.Compilation.Assembly )
                {
                    // The type is defined in a different assembly.
                    return;
                }

                if ( types.Add( type ) )
                {
                    if ( type.BaseType != null && type.ContainingAssembly == type.BaseType.ContainingAssembly )
                    {
                        AddTypeRecursive( type.BaseType );
                    }

                    foreach ( var interfaceImpl in type.Interfaces )
                    {
                        AddTypeRecursive( interfaceImpl );
                    }
                }
                else
                {
                    // The type was already processed.
                }
            }

            foreach ( var typeNode in findTypesVisitor.Types )
            {
                var type = (INamedTypeSymbol?) semanticModel.GetDeclaredSymbol( typeNode );

                if ( type == null )
                {
                    continue;
                }

                AddTypeRecursive( type );
            }

            return new PartialCompilationModel( (CSharpCompilation) semanticModel.Compilation, types );
        }
    }
}