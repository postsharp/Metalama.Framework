// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Concurrent;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking
{
    internal class LinkerAspectReferenceSyntaxProvider : AspectReferenceSyntaxProvider
    {
        public override ExpressionSyntax GetFinalizerReference( AspectLayerId aspectLayer, IMethod overriddenFinalizer, OurSyntaxGenerator syntaxGenerator )
            => InvocationExpression(
                MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        HelperTypeName,
                        GenericName(
                            Identifier( "Finalizer" ),
                            TypeArgumentList(
                                SingletonSeparatedList( syntaxGenerator.Type( overriddenFinalizer.DeclaringType.GetSymbol().AssertNotNull() ) ) ) ) )
                    .WithAspectReferenceAnnotation(
                        aspectLayer,
                        AspectReferenceOrder.Base,
                        AspectReferenceTargetKind.Self,
                        flags: AspectReferenceFlags.Inlineable ) );

        public override ExpressionSyntax GetOperatorReference( AspectLayerId aspectLayer, IMethod overriddenOperator, OurSyntaxGenerator syntaxGenerator )
        {
            return
                InvocationExpression(
                    MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            HelperTypeName,
                            GenericName(
                                Identifier( overriddenOperator.OperatorKind.ToOperatorMethodName() ),
                                TypeArgumentList(
                                    SingletonSeparatedList( syntaxGenerator.Type( overriddenOperator.DeclaringType.GetSymbol().AssertNotNull() ) ) ) ) )
                        .WithAspectReferenceAnnotation(
                            aspectLayer,
                            AspectReferenceOrder.Base,
                            AspectReferenceTargetKind.Self,
                            flags: AspectReferenceFlags.Inlineable ),
                    syntaxGenerator.ArgumentList(overriddenOperator, p => IdentifierName(p.Name)))
                ;
        }

        private static NameSyntax HelperTypeName => IdentifierName( "__LinkerIntroductionHelpers__" );

        private static readonly ConcurrentDictionary<LanguageVersion, SyntaxTree> _linkerHelperSyntaxTreeCache = new();

        public static SyntaxTree GetLinkerHelperSyntaxTree( LanguageVersion languageVersion )
            => _linkerHelperSyntaxTreeCache.GetOrAdd( languageVersion, GetLinkerHelperSyntaxTreeCode );

        private static SyntaxTree GetLinkerHelperSyntaxTreeCode( LanguageVersion v )
        {
            var code = @"
internal class __LinkerIntroductionHelpers__
{
    public static void Finalizer<T>() {}
}
                ";

            return CSharpSyntaxTree.ParseText(
                code,
                path: "__LinkerIntroductionHelpers__.cs",
                encoding: Encoding.UTF8,
                options: CSharpParseOptions.Default.WithLanguageVersion( v ) );
        }
    }
}