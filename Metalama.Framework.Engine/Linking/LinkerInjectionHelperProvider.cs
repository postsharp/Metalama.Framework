// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using OperatorKind = Metalama.Framework.Code.OperatorKind;

#pragma warning disable CA1822

namespace Metalama.Framework.Engine.Linking
{
    internal sealed class LinkerInjectionHelperProvider
    {
        public const string HelperTypeName = "__LinkerInjectionHelpers__";
        public const string FinalizeMemberName = "__Finalize";
        public const string PropertyMemberName = "__Property";
        private const string _eventFieldInitializationExpressionMemberName = "__EventFieldInitializationExpression__";
        private const string _emptyCodeTypeName = "__Empty";
        private const string _sourceCodeTypeName = "__Source";
        private const string _overridenByTypeName = "__OverriddenBy";
        private const string _ordinalTypeName = "__Ordinal";
        private const string _compositeOrdinalTypeName = "__CompositeOrdinal";
        private const string _syntaxTreeName = "__LinkerInjectionHelpers__.cs";

        private static readonly ConcurrentDictionary<LanguageOptions, SyntaxTree> _linkerHelperSyntaxTreeCache = new();

        private readonly bool _useNullability;
        private readonly CompilationModel _finalCompilationModel;

        public LinkerInjectionHelperProvider( CompilationModel finalCompilationModel, bool useNullability )
        {
            // TODO: Usage of nullability should be determined from context (design time).
            this._finalCompilationModel = finalCompilationModel;
            this._useNullability = useNullability;
        }

        public static ExpressionSyntax GetFinalizeMemberExpression()
        {
            return
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName( HelperTypeName ),
                    IdentifierName( FinalizeMemberName ) );
        }

        public static ExpressionSyntax GetPropertyMemberExpression()
        {
            return
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName( HelperTypeName ),
                    IdentifierName( PropertyMemberName ) );
        }

        public static ExpressionSyntax GetEventFieldInitializerExpressionMemberExpression()
        {
            return
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName( HelperTypeName ),
                    IdentifierName( _eventFieldInitializationExpressionMemberName ) );
        }

        public static ExpressionSyntax GetOperatorMemberExpression(
            OurSyntaxGenerator syntaxGenerator,
            OperatorKind operatorKind,
            IType returnType,
            IEnumerable<IType> parameterTypes )
        {
            return
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName( HelperTypeName ),
                    GenericName(
                        Identifier( operatorKind.ToOperatorMethodName() ),
                        TypeArgumentList(
                            SeparatedList(
                                parameterTypes.Select( p => syntaxGenerator.Type( p.GetSymbol().AssertNotNull() ) )
                                    .Append( syntaxGenerator.Type( returnType.GetSymbol().AssertNotNull() ) ) ) ) ) );
        }

        public TypeSyntax GetOverriddenByType( OurSyntaxGenerator syntaxGenerator, IAspectClass aspectType, int ordinal )
        {
            var aspectTypeSyntax = syntaxGenerator.Type( this._finalCompilationModel.Factory.GetTypeByReflectionType( aspectType.Type ).GetSymbol() );

            switch ( ordinal )
            {
                case 0:
                    return
                        QualifiedName(
                            IdentifierName( HelperTypeName ),
                            GenericName(
                                Identifier( _overridenByTypeName ),
                                TypeArgumentList( SingletonSeparatedList( aspectTypeSyntax ) ) ) );

                case < 10:
                    return
                        QualifiedName(
                            IdentifierName( HelperTypeName ),
                            GenericName(
                                Identifier( _overridenByTypeName ),
                                TypeArgumentList(
                                    SeparatedList(
                                        new[]
                                        {
                                            aspectTypeSyntax,
                                            QualifiedName(
                                                IdentifierName( HelperTypeName ),
                                                IdentifierName( _ordinalTypeName + ordinal ) )
                                        } ) ) ) );

                case < 100:
                    return
                        QualifiedName(
                            IdentifierName( HelperTypeName ),
                            GenericName(
                                Identifier( _overridenByTypeName ),
                                TypeArgumentList(
                                    SeparatedList(
                                        new[]
                                        {
                                            aspectTypeSyntax,
                                            QualifiedName(
                                                IdentifierName( HelperTypeName ),
                                                GenericName(
                                                    Identifier( _compositeOrdinalTypeName ),
                                                    TypeArgumentList(
                                                        SeparatedList<TypeSyntax>(
                                                            new[]
                                                            {
                                                                QualifiedName(
                                                                    IdentifierName( HelperTypeName ),
                                                                    IdentifierName( _ordinalTypeName + (ordinal / 10) ) ),
                                                                QualifiedName(
                                                                    IdentifierName( HelperTypeName ),
                                                                    IdentifierName( _ordinalTypeName + (ordinal % 10) ) )
                                                            } ) ) ) )
                                        } ) ) ) );

                default:
                    // NOTE: Lets have a beer when someone really hits this limit (without having a bug in the aspect).
                    throw new AssertionFailedException( $"More than 100 overrides of a single member by aspect {aspectType.ShortName}." );
            }
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
    public static ref T{suffix} {PropertyMemberName}<T>(T{suffix} value) => ref Dummy<T{suffix}>.Field;
    public static void {_eventFieldInitializationExpressionMemberName}<T>(T? value) where T : System.Delegate {{}}
    {string.Join( "\n    ", binaryOperators )}
    {string.Join( "\n    ", unaryOperators )}
    {string.Join( "\n    ", conversionOperators )}

    public readonly struct {_emptyCodeTypeName} {{}}
    public readonly struct {_sourceCodeTypeName} {{}}
    public readonly struct {_overridenByTypeName}<TAspect> {{}}
    public readonly struct {_overridenByTypeName}<TAspect, TOrdinal> {{}}

    public readonly struct {_ordinalTypeName}0 {{}}
    public readonly struct {_ordinalTypeName}1 {{}}
    public readonly struct {_ordinalTypeName}2 {{}}
    public readonly struct {_ordinalTypeName}3 {{}}
    public readonly struct {_ordinalTypeName}4 {{}}
    public readonly struct {_ordinalTypeName}5 {{}}
    public readonly struct {_ordinalTypeName}6 {{}}
    public readonly struct {_ordinalTypeName}7 {{}}
    public readonly struct {_ordinalTypeName}8 {{}}
    public readonly struct {_ordinalTypeName}9 {{}}
    public readonly struct {_compositeOrdinalTypeName}<T1, T2> {{}}

    public class Dummy<T>
    {{
        public static T? Field;
    }}
}}
                ";

            return CSharpSyntaxTree.ParseText(
                code,
                path: _syntaxTreeName,
                encoding: Encoding.UTF8,
                options: options.ToParseOptions() );
        }
    }
}