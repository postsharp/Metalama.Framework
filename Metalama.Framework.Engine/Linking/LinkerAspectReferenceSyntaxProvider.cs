// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking
{
    internal class LinkerAspectReferenceSyntaxProvider : AspectReferenceSyntaxProvider
    {
        private static NameSyntax HelperTypeName => IdentifierName( "__LinkerIntroductionHelpers__" );

        private static readonly ConcurrentDictionary<LanguageVersion, SyntaxTree> _linkerHelperSyntaxTreeCache = new();

        private readonly bool _useNullability;

        public LinkerAspectReferenceSyntaxProvider( bool useNullability )
        {
            this._useNullability = useNullability;
        }

        public override ExpressionSyntax GetFinalizerReference( AspectLayerId aspectLayer, IMethod overriddenFinalizer, OurSyntaxGenerator syntaxGenerator )
            => InvocationExpression(
                MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        HelperTypeName,
                        IdentifierName( "Finalizer" ) )
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
                                    SeparatedList(
                                        overriddenOperator.Parameters.Select( p => syntaxGenerator.Type( p.Type.GetSymbol().AssertNotNull() ) )
                                        .Append( syntaxGenerator.Type( overriddenOperator.ReturnType.GetSymbol().AssertNotNull() ) ) ) ) ) )
                        .WithAspectReferenceAnnotation(
                            aspectLayer,
                            AspectReferenceOrder.Base,
                            AspectReferenceTargetKind.Self,
                            flags: AspectReferenceFlags.Inlineable ),
                    syntaxGenerator.ArgumentList( overriddenOperator, p => IdentifierName( p.Name ) ) );
        }

        public SyntaxTree? GetLinkerHelperSyntaxTree( LanguageVersion languageVersion )
            => _linkerHelperSyntaxTreeCache.GetOrAdd( languageVersion, this.GetLinkerHelperSyntaxTreeCode );

        private SyntaxTree? GetLinkerHelperSyntaxTreeCode( LanguageVersion v )
        {
            var useNullability = this._useNullability && v is LanguageVersion.CSharp9 or LanguageVersion.CSharp10;
            var suffix = useNullability ? "?" : "";

            var binaryOperators = 
                Enum.GetValues( typeof( OperatorKind ) )
                .Cast<OperatorKind>()
                .Where( op => op.IsBinaryOperator() )
                .Select( op => $"public static R{suffix} {op.ToOperatorMethodName()}<A,B,R>(A{suffix} a, B{suffix} b) => default(R{suffix});" );

            var unaryOperators =
                Enum.GetValues( typeof( OperatorKind ) )
                .Cast<OperatorKind>()
                .Where( op => op.IsUnaryOperator() )
                .Select( op => $"public static R{suffix} {op.ToOperatorMethodName()}<A,R>(A{suffix} a) => default(R{suffix});" );

            var conversionOperators =
                Enum.GetValues( typeof( OperatorKind ) )
                .Cast<OperatorKind>()
                .Where( op => op.IsConversionOperator() )
                .Select( op => $"public static R{suffix} {op.ToOperatorMethodName()}<A,R>(A{suffix} a) => default(R{suffix});" );

            var code = @$"
{(useNullability ? "#nullable enable" : "")}
internal class __LinkerIntroductionHelpers__
{{
    public static void Finalizer() {{}}
    {string.Join( "\n    ", binaryOperators )}
    {string.Join( "\n    ", unaryOperators )}
    {string.Join( "\n    ", conversionOperators )}
}}
                ";

            return CSharpSyntaxTree.ParseText(
                code,
                path: "__LinkerIntroductionHelpers__.cs",
                encoding: Encoding.UTF8,
                options: CSharpParseOptions.Default.WithLanguageVersion( v ) );
        }
    }
}