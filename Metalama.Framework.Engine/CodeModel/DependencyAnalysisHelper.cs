// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel
{
    public static partial class DependencyAnalysisHelper
    {
        public static void FindDeclaredAndAttributeTypes(
            SemanticModel semanticModel,
            Action<INamedTypeSymbol> addDeclaredType,
            Action<INamedTypeSymbol> addAttributeType )
        {
            var visitor = new FindDeclaredAndAttributeTypesVisitor( semanticModel, addDeclaredType, addAttributeType );
            visitor.Visit( semanticModel.SyntaxTree.GetRoot() );
        }

        public static void FindDeclaredTypes( ISemanticModel semanticModel, Action<INamedTypeSymbol> addDeclaredType )
        {
            var visitor = new FindDeclaredTypesVisitor( semanticModel, addDeclaredType );
            visitor.Visit( semanticModel.SyntaxTree.GetRoot() );
        }
    }
}