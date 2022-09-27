// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
        public const string HelperTypeName = "__LinkerIntroductionHelpers__";
        public const string FinalizeMemberName = "__Finalize";
        public const string PropertyMemberName = "__Property";
        public const string SyntaxTreeName = "__LinkerIntroductionHelpers__.cs";

        private static readonly ConcurrentDictionary<LanguageOptions, SyntaxTree> _linkerHelperSyntaxTreeCache = new();

        private readonly bool _useNullability;

        public LinkerAspectReferenceSyntaxProvider( bool useNullability )
        {
            // TODO: Usage of nullability should be determined from context (design time).
            this._useNullability = useNullability;
        }

        public override ExpressionSyntax GetFinalizerReference( AspectLayerId aspectLayer, IMethod overriddenFinalizer, OurSyntaxGenerator syntaxGenerator )
            => InvocationExpression(
                MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName( HelperTypeName ),
                        IdentifierName( FinalizeMemberName ) )
                    .WithAspectReferenceAnnotation(
                        aspectLayer,
                        AspectReferenceOrder.Base,
                        AspectReferenceTargetKind.Self,
                        flags: AspectReferenceFlags.Inlineable ) );

        public override ExpressionSyntax GetPropertyReference( AspectLayerId aspectLayer, IProperty overriddenProperty, OurSyntaxGenerator syntaxGenerator )
            => InvocationExpression(
                MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName( HelperTypeName ),
                        IdentifierName( PropertyMemberName ) )
                    .WithAspectReferenceAnnotation(
                        aspectLayer,
                        AspectReferenceOrder.Base,
                        AspectReferenceTargetKind.Self,
                        flags: AspectReferenceFlags.Inlineable ),
                ArgumentList(
                    SingletonSeparatedList(
                        Argument(
                            overriddenProperty.IsStatic
                            ? MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                syntaxGenerator.Type( overriddenProperty.DeclaringType.GetSymbol() ),
                                IdentifierName( overriddenProperty.Name ) )
                            : MemberAccessExpression( 
                                SyntaxKind.SimpleMemberAccessExpression,
                                ThisExpression(),
                                IdentifierName( overriddenProperty.Name ) ) ) ) ) );

        public override ExpressionSyntax GetOperatorReference( AspectLayerId aspectLayer, IMethod overriddenOperator, OurSyntaxGenerator syntaxGenerator )
        {
            return
                InvocationExpression(
                    MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName( HelperTypeName ),
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

        public SyntaxTree GetLinkerHelperSyntaxTree( LanguageOptions options )
            => _linkerHelperSyntaxTreeCache.GetOrAdd( options, this.GetLinkerHelperSyntaxTreeCore );

        private SyntaxTree GetLinkerHelperSyntaxTreeCore( LanguageOptions options )
        {
            var useNullability = this._useNullability && options.Version is LanguageVersion.CSharp9 or LanguageVersion.CSharp10;
            var suffix = useNullability ? "?" : "";

            var binaryOperators =
                Enum.GetValues( typeof(OperatorKind) )
                    .Cast<OperatorKind>()
                    .Where( op => op.GetCategory() == OperatorCategory.Binary )
                    .Select( op => $"public static R{suffix} {op.ToOperatorMethodName()}<A,B,R>(A{suffix} a, B{suffix} b) => default(R{suffix});" );

            var unaryOperators =
                Enum.GetValues( typeof(OperatorKind) )
                    .Cast<OperatorKind>()
                    .Where( op => op.GetCategory() == OperatorCategory.Unary )
                    .Select( op => $"public static R{suffix} {op.ToOperatorMethodName()}<A,R>(A{suffix} a) => default(R{suffix});" );

            var conversionOperators =
                Enum.GetValues( typeof(OperatorKind) )
                    .Cast<OperatorKind>()
                    .Where( op => op.GetCategory() == OperatorCategory.Conversion )
                    .Select( op => $"public static R{suffix} {op.ToOperatorMethodName()}<A,R>(A{suffix} a) => default(R{suffix});" );

            var code = @$"
{(useNullability ? "#nullable enable" : "")}
internal class {HelperTypeName}
{{
    public static void {FinalizeMemberName}() {{}}
    public static ref T {PropertyMemberName}<T>(T value) => ref Dummy<T>.Field;
    {string.Join( "\n    ", binaryOperators )}
    {string.Join( "\n    ", unaryOperators )}
    {string.Join( "\n    ", conversionOperators )}

    public class Dummy<T>
    {{
        public static T? Field;
    }}
}}
                ";

            return CSharpSyntaxTree.ParseText(
                code,
                path: SyntaxTreeName,
                encoding: Encoding.UTF8,
                options: options.ToParseOptions() );
        }
    }
}