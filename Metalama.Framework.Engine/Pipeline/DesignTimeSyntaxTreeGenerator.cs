// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Linking;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace Metalama.Framework.Engine.Pipeline
{
    internal static class DesignTimeSyntaxTreeGenerator
    {
        public static void GenerateDesignTimeSyntaxTrees(
            PartialCompilation partialCompilation,
            CompilationModel compilationModel,
            IServiceProvider serviceProvider,
            UserDiagnosticSink diagnostics,
            CancellationToken cancellationToken,
            out IReadOnlyList<IntroducedSyntaxTree> additionalSyntaxTrees )
        {
            var transformations = compilationModel.GetAllObservableTransformations( true );

            var additionalSyntaxTreeList = new List<IntroducedSyntaxTree>();
            additionalSyntaxTrees = additionalSyntaxTreeList;

            LexicalScopeFactory lexicalScopeFactory = new( compilationModel );
            var introductionNameProvider = new LinkerIntroductionNameProvider();

            foreach ( var transformationGroup in transformations )
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ( transformationGroup.DeclaringDeclaration is not INamedType declaringType )
                {
                    // We only support introductions to types.
                    continue;
                }

                if ( !declaringType.IsPartial )
                {
                    // If the type is not marked as partial, we can emit a diagnostic and a code fix, but not a partial class itself.
                    diagnostics.Report(
                        GeneralDiagnosticDescriptors.TypeNotPartial.CreateRoslynDiagnostic( declaringType.GetDiagnosticLocation(), declaringType ) );

                    continue;
                }

                // TODO: support struct, record.

                // Create a class.
                var classDeclaration = SyntaxFactory.ClassDeclaration(
                    default,
                    SyntaxTokenList.Create( SyntaxFactory.Token( SyntaxKind.PartialKeyword ) ),
                    SyntaxFactory.Identifier( declaringType.Name ),
                    null,
                    null,
                    default,
                    default );

                // Add members to the class.
                var syntaxGenerationContext = SyntaxGenerationContext.CreateDefault( serviceProvider, partialCompilation.Compilation, true );

                foreach ( var transformation in transformationGroup.Transformations )
                {
                    if ( transformation is IMemberIntroduction memberIntroduction )
                    {
                        // TODO: Provide other implementations or allow nulls (because this pipeline should not execute anything).
                        // TODO: Implement support for initializable transformations.
                        var introductionContext = new MemberIntroductionContext(
                            diagnostics,
                            introductionNameProvider,
                            lexicalScopeFactory,
                            syntaxGenerationContext,
                            serviceProvider,
                            null,
                            ImmutableDictionary<IHierarchicalTransformation, TransformationInitializationResult?>.Empty );

                        var introducedMembers = memberIntroduction.GetIntroducedMembers( introductionContext )
                            .Select( m => m.Syntax.NormalizeWhitespace() )
                            .ToArray();

                        classDeclaration = classDeclaration.AddMembers( introducedMembers );
                    }

                    if ( transformation is IIntroducedInterface interfaceImplementation )
                    {
                        classDeclaration = classDeclaration.AddBaseListTypes( interfaceImplementation.GetSyntax() );
                    }
                }

                // Add the class to a namespace.
                SyntaxNode topDeclaration = classDeclaration;

                if ( !declaringType.Namespace.IsGlobalNamespace )
                {
                    topDeclaration = SyntaxFactory.NamespaceDeclaration(
                        SyntaxFactory.ParseName( declaringType.Namespace.FullName ),
                        default,
                        default,
                        SyntaxFactory.SingletonList<MemberDeclarationSyntax>( classDeclaration ) );
                }

                // Choose the best syntax tree
                var originalSyntaxTree = ((IDeclarationImpl) declaringType).DeclaringSyntaxReferences.Select( r => r.SyntaxTree )
                    .OrderBy( s => s.FilePath.Length )
                    .First();

                var generatedSyntaxTree = SyntaxFactory.SyntaxTree( topDeclaration.NormalizeWhitespace(), encoding: Encoding.UTF8 );
                var syntaxTreeName = declaringType.FullName + ".cs";

                additionalSyntaxTreeList.Add( new IntroducedSyntaxTree( syntaxTreeName, originalSyntaxTree, generatedSyntaxTree ) );
            }
        }
    }
}