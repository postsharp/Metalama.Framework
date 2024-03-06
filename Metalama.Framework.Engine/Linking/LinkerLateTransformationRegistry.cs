﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
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
        private readonly ISet<ISymbol> _primaryConstructorInitializedMembers;

        public LinkerLateTransformationRegistry( 
            PartialCompilation intermediateCompilation,
            IReadOnlyDictionary<INamedType, LateTypeLevelTransformations> lateTypeLevelTransformations )
        {
            // TODO: Parallelize.
            HashSet<INamedTypeSymbol> typesWithRemovedPrimaryConstructor;
            HashSet<ISymbol> primaryConstructorInitializedMembers;

            this._typesWithRemovedPrimaryConstructor = typesWithRemovedPrimaryConstructor = new HashSet<INamedTypeSymbol>( intermediateCompilation.CompilationContext.SymbolComparer );
            this._primaryConstructorInitializedMembers = primaryConstructorInitializedMembers = new HashSet<ISymbol>( intermediateCompilation.CompilationContext.SymbolComparer );

            foreach (var lateTypeLevelTransformationPair in lateTypeLevelTransformations )
            {
                var type = lateTypeLevelTransformationPair.Key;
                var transformations = lateTypeLevelTransformationPair.Value;

                var typeSymbol = intermediateCompilation.CompilationContext.SymbolTranslator.Translate( type.GetSymbol().AssertNotNull() ).AssertNotNull();

                if (transformations.ShouldRemovePrimaryConstructor)
                {
                    typesWithRemovedPrimaryConstructor.Add( typeSymbol );

                    foreach ( var symbol in typeSymbol.GetMembers() )
                    {
                        if ( symbol.IsImplicitlyDeclared )
                        {
                            continue;
                        }

                        switch ( symbol )
                        {
                            case IFieldSymbol fieldSymbol:
                                var declarator = (VariableDeclaratorSyntax) fieldSymbol.GetPrimaryDeclaration().AssertNotNull();

                                if ( declarator.Initializer == null )
                                {
                                    continue;
                                }

                                primaryConstructorInitializedMembers.Add( fieldSymbol );

                                break;

                            case IPropertySymbol propertySymbol:
                                var primaryDeclaration = propertySymbol.GetPrimaryDeclaration().AssertNotNull();

                                switch ( primaryDeclaration )
                                {
                                    case PropertyDeclarationSyntax propertyDeclaration:
                                        if ( propertyDeclaration.Initializer == null )
                                        {
                                            continue;
                                        }

                                        primaryConstructorInitializedMembers.Add( propertySymbol );

                                        break;

                                    case ParameterSyntax:
                                        primaryConstructorInitializedMembers.Add( propertySymbol );

                                        break;
                                }

                                break;

                            case IEventSymbol eventSymbol:
                                var eventDeclaration = eventSymbol.GetPrimaryDeclaration().AssertNotNull();

                                if ( eventDeclaration is VariableDeclaratorSyntax eventFieldDeclarator )
                                {

                                    if ( eventFieldDeclarator.Initializer == null )
                                    {
                                        continue;
                                    }

                                    primaryConstructorInitializedMembers.Add( eventSymbol );
                                }

                                break;
                        }
                    }
                }
            }
        }

        public bool HasRemovedPrimaryConstructor(INamedTypeSymbol type)
        {
            return this._typesWithRemovedPrimaryConstructor.Contains(type);
        }

#pragma warning disable CA1822 // Mark members as static
        public IReadOnlyList<IFieldSymbol> GetPrimaryConstructorFields( INamedTypeSymbol type )
#pragma warning restore CA1822 // Mark members as static
        {
#if ROSLYN_4_8_0_OR_GREATER
            var typeSyntax =
                (TypeDeclarationSyntax) type.DeclaringSyntaxReferences.Select( r => r.GetSyntax() )
                .Single( d => d is TypeDeclarationSyntax { ParameterList: not null } );

            if (typeSyntax is RecordDeclarationSyntax)
            {
                return Array.Empty<IFieldSymbol>();
            }

            var parameterList = typeSyntax.ParameterList.AssertNotNull();

            return type.GetMembers().OfType<IFieldSymbol>().Where( f => f.Locations.Any( l => parameterList.Span.Contains( l.SourceSpan.Start ) ) ).ToArray();
#else
            return Array.Empty<IFieldSymbol>();
#endif
        }

#pragma warning disable CA1822 // Mark members as static
        public IReadOnlyList<IPropertySymbol> GetPrimaryConstructorProperties( INamedTypeSymbol type )
#pragma warning restore CA1822 // Mark members as static
        {
            return type.GetMembers().OfType<IPropertySymbol>().Where( p => p.GetPrimaryDeclaration() is ParameterSyntax ).ToArray();
        }

        public bool IsPrimaryConstructorInitializedMember( ISymbol symbol )
        {
            return this._primaryConstructorInitializedMembers.Contains( symbol );
        }

        public ArgumentListSyntax? GetPrimaryConstructorBaseArgumentList( IMethodSymbol constructor )
        {
            var type = constructor.ContainingType;

            Invariant.Assert( this.HasRemovedPrimaryConstructor( type ) );

#if ROSLYN_4_8_0_OR_GREATER
            var typeSyntax =
                (TypeDeclarationSyntax) type.DeclaringSyntaxReferences.Select( r => r.GetSyntax() )
                .Single( d => d is TypeDeclarationSyntax { ParameterList: not null } );
#else
            var typeSyntax =
                (RecordDeclarationSyntax) type.DeclaringSyntaxReferences.Select( r => r.GetSyntax() )
                .Single( d => d is RecordDeclarationSyntax { ParameterList: not null } );
#endif

            var primaryConstructorBase = typeSyntax.BaseList?.Types.OfType<PrimaryConstructorBaseTypeSyntax>().SingleOrDefault();

            return primaryConstructorBase?.ArgumentList;
        }
    }
}