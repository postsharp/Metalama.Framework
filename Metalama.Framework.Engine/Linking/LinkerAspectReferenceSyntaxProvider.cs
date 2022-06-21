// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking
{
    internal class LinkerAspectReferenceSyntaxProvider : AspectReferenceSyntaxProvider
    {
        public LinkerAspectReferenceSyntaxProvider() { }

        public override ExpressionSyntax GetFinalizerReference( AspectLayerId aspectLayer, IMethod overriddenFinalizer, OurSyntaxGenerator syntaxGenerator )
        {
            return
                InvocationExpression(
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
                            flags: AspectReferenceFlags.Inlineable ) )
                ;
        }

        private static NameSyntax HelperTypeName => IdentifierName( "__LinkerIntroductionHelpers__" );

        private static SyntaxTree? _linkerHelperSyntaxTree;

        public static SyntaxTree LinkerHelperSyntaxTree
        {
            get
            {
                if ( _linkerHelperSyntaxTree == null )
                {
                    var code = @"
internal class __LinkerIntroductionHelpers__
{
    public static void Finalizer<T>() {}
}
                ";

                    _linkerHelperSyntaxTree = CSharpSyntaxTree.ParseText( code, path: "__LinkerIntroductionHelpers__.cs", encoding: Encoding.UTF8 );
                }

                return _linkerHelperSyntaxTree;
            }
        }
    }
}