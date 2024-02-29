// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Linking
{
    /// <summary>
    /// Stores information about transformations that are unrelated to linking but has to be performed during linking. 
    /// </summary>
    internal class LinkerLateTransformationRegistry
    {
        private readonly ISet<INamedTypeSymbol> _typesWithRemovedPrimaryConstructor;

        public LinkerLateTransformationRegistry( 
            PartialCompilation intermediateCompilation,
            IReadOnlyDictionary<INamedType, LateTypeLevelTransformations> lateTypeLevelTransformations )
        {
            HashSet<INamedTypeSymbol> typesWithRemovedPrimaryConstructor;

            this._typesWithRemovedPrimaryConstructor = typesWithRemovedPrimaryConstructor = new HashSet<INamedTypeSymbol>( intermediateCompilation.CompilationContext.SymbolComparer );

            foreach (var lateTypeLevelTransformationPair in lateTypeLevelTransformations )
            {
                var type = lateTypeLevelTransformationPair.Key;
                var transformations = lateTypeLevelTransformationPair.Value;

                var typeSymbol = intermediateCompilation.CompilationContext.SymbolTranslator.Translate( type.GetSymbol().AssertNotNull() ).AssertNotNull();

                if (transformations.ShouldRemovePrimaryConstructor)
                {
                    typesWithRemovedPrimaryConstructor.Add( typeSymbol );
                }
            }
        }

        public bool HasRemovedPrimaryConstructor(INamedTypeSymbol type)
        {
            return this._typesWithRemovedPrimaryConstructor.Contains(type);
        }

        public IReadOnlyList<IFieldSymbol> GetPrimaryConstructorFields( INamedTypeSymbol type )
        {
            return type.GetMembers().OfType<IFieldSymbol>().Where(f => f.Name is [ '<', .., '>', 'P' ] ).ToArray();
        }

        public ArgumentListSyntax? GetPrimaryConstructorBaseArgumentList( IMethodSymbol constructor )
        {
            var type = constructor.ContainingType;

            Invariant.Assert( this.HasRemovedPrimaryConstructor( type ) );

            var typeSyntax = (TypeDeclarationSyntax) type.GetPrimaryDeclaration().AssertNotNull();

            var primaryConstructorBase = typeSyntax.BaseList?.Types.OfType<PrimaryConstructorBaseTypeSyntax>().SingleOrDefault();

            return primaryConstructorBase?.ArgumentList;
        }
    }
}