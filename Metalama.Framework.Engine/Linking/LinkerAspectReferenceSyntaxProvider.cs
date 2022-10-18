// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Pseudo;
using Metalama.Framework.Engine.Templating;
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

        public override ExpressionSyntax GetPropertyReference(
            AspectLayerId aspectLayer,
            IProperty overriddenProperty,
            AspectReferenceTargetKind targetKind,
            OurSyntaxGenerator syntaxGenerator )
        {
            switch (targetKind, overriddenProperty)
            {
                case (AspectReferenceTargetKind.PropertySetAccessor, { SetMethod: IPseudoDeclaration }):
                case (AspectReferenceTargetKind.PropertyGetAccessor, { GetMethod: IPseudoDeclaration }):
                    // For pseudo source: __LinkerIntroductionHelpers__.__Property(<property_expression>)
                    // It is important to track the <property_expression>.
                    var symbolSourceExpression = CreateMemberAccessExpression( overriddenProperty, syntaxGenerator );

                    return
                        InvocationExpression(
                            MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName( HelperTypeName ),
                                    IdentifierName( PropertyMemberName ) )
                                .WithAspectReferenceAnnotation(
                                    aspectLayer,
                                    AspectReferenceOrder.Base,
                                    targetKind,
                                    flags: AspectReferenceFlags.Inlineable ),
                            ArgumentList( SingletonSeparatedList( Argument( symbolSourceExpression ) ) ) )
                        ;

                default:
                    // Otherwise: <property_expression>
                    return
                        CreateMemberAccessExpression( overriddenProperty, syntaxGenerator )
                            .WithAspectReferenceAnnotation(
                                aspectLayer,
                                AspectReferenceOrder.Base,
                                targetKind,
                                AspectReferenceFlags.Inlineable );
            }
        }

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

        private static ExpressionSyntax CreateMemberAccessExpression( IMember overriddenDeclaration, OurSyntaxGenerator syntaxGenerator )
        {
            ExpressionSyntax expression;

            var memberNameString =
                overriddenDeclaration switch
                {
                    { IsExplicitInterfaceImplementation: true } => overriddenDeclaration.Name.Split( '.' ).Last(),
                    _ => overriddenDeclaration.Name
                };

            SimpleNameSyntax memberName;

            if ( overriddenDeclaration is IGeneric generic && generic.TypeParameters.Count > 0 )
            {
                memberName = GenericName( memberNameString )
                    .WithTypeArgumentList( TypeArgumentList( SeparatedList( generic.TypeParameters.Select( p => (TypeSyntax) IdentifierName( p.Name ) ) ) ) );
            }
            else
            {
                memberName = IdentifierName( memberNameString );
            }

            if ( !overriddenDeclaration.IsStatic )
            {
                if ( overriddenDeclaration.IsExplicitInterfaceImplementation )
                {
                    var implementedInterfaceMember = overriddenDeclaration.GetExplicitInterfaceImplementation();

                    expression = MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ParenthesizedExpression(
                            SyntaxFactoryEx.SafeCastExpression(
                                syntaxGenerator.Type( implementedInterfaceMember.DeclaringType.GetSymbol() ),
                                ThisExpression() ) ),
                        memberName );
                }
                else
                {
                    expression = MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ThisExpression(),
                        memberName );
                }
            }
            else
            {
                expression =
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        syntaxGenerator.Type( overriddenDeclaration.DeclaringType.GetSymbol() ),
                        memberName );
            }

            return expression;
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