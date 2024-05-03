// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;
using System.Reflection;

namespace Metalama.Framework.Engine.SyntaxGeneration;

/// <summary>
/// Limited version of Roslyn <see cref="SyntaxGenerator"/> that works with <see cref="IType"/> instead of <see cref="ISymbol"/>.
/// </summary>
internal partial class SyntaxGeneratorForIType
{
    public static SyntaxGenerator RoslynSyntaxGenerator { get; }

    static SyntaxGeneratorForIType()
    {
        var type = WorkspaceHelper.CSharpWorkspacesAssembly.GetType( "Microsoft.CodeAnalysis.CSharp.CodeGeneration.CSharpSyntaxGenerator" )!;
        var field = type.GetField( "Instance", BindingFlags.Public | BindingFlags.Static )!;
        RoslynSyntaxGenerator = (SyntaxGenerator) field.GetValue( null ).AssertNotNull();
    }

    private readonly SyntaxGenerationOptions _generationOptions;
    private readonly TypeSyntaxGeneratorVisitor _typeSyntaxGeneratorVisitor;

    public SyntaxGeneratorForIType( SyntaxGenerationOptions generationOptions )
    {
        this._generationOptions = generationOptions;
        this._typeSyntaxGeneratorVisitor = new TypeSyntaxGeneratorVisitor( this );
    }

    // Based on Roslyn ITypeSymbolExtensions.GenerateTypeSyntax.
    internal TypeSyntax TypeExpression( IType type )
    {
        var syntax = this._typeSyntaxGeneratorVisitor.Visit( type )
            .WithAdditionalAnnotations( Simplifier.Annotation );

        if ( type.IsReferenceType == true )
        {
            var additionalAnnotation = type.IsNullable switch
            {
                null => NullableSyntaxAnnotationEx.Oblivious,
                true or false => NullableSyntaxAnnotationEx.AnnotatedOrNotAnnotated,
            };

            if ( additionalAnnotation is not null )
            {
                syntax = syntax.WithAdditionalAnnotations( additionalAnnotation );
            }
        }

        return syntax;
    }

    // Copy of Microsoft.CodeAnalysis.CSharp.Shared.Lightup.NullableSyntaxAnnotationEx.
    private static class NullableSyntaxAnnotationEx
    {
        public static SyntaxAnnotation? Oblivious { get; }

        public static SyntaxAnnotation? AnnotatedOrNotAnnotated { get; }

        static NullableSyntaxAnnotationEx()
        {
            var nullableSyntaxAnnotation = typeof(Workspace).Assembly.GetType(
                "Microsoft.CodeAnalysis.CodeGeneration.NullableSyntaxAnnotation",
                throwOnError: false );

            if ( nullableSyntaxAnnotation is object )
            {
                Oblivious = (SyntaxAnnotation?) nullableSyntaxAnnotation.GetField( nameof(Oblivious), BindingFlags.Static | BindingFlags.Public )
                    ?.GetValue( null );

                AnnotatedOrNotAnnotated = (SyntaxAnnotation?) nullableSyntaxAnnotation
                    .GetField( nameof(AnnotatedOrNotAnnotated), BindingFlags.Static | BindingFlags.Public )
                    ?.GetValue( null );
            }
        }
    }
}