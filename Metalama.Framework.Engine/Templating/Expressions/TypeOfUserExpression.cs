// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Templating.Expressions
{
    internal sealed class TypeOfUserExpression : UserExpression
    {
        private readonly IType _type;

        public TypeOfUserExpression( IType type )
        {
            this._type = type;
        }

        protected override ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext )
        {
            var typeExpression =
                this._type switch
                {
                    // Generic type definition has to have omitted arguments in "typeof".
                    INamedType { IsCanonicalGenericInstance: true, IsGeneric: true } =>
                        syntaxGenerationContext.SyntaxGenerator.Type( this._type.GetSymbol() ) switch
                        {
                            // [alias::]Namespace.Type<T,...>
                            QualifiedNameSyntax { Right: GenericNameSyntax genericName } qualifiedName =>
                                qualifiedName.WithRight(
                                    genericName.WithTypeArgumentList(
                                        TypeArgumentList(
                                            SeparatedList<TypeSyntax>(
                                                genericName.TypeArgumentList.Arguments.SelectAsEnumerable( _ => OmittedTypeArgument() ) ) ) ) ),
                            
                            // Type<T,...>
                            GenericNameSyntax genericName =>
                                genericName.WithTypeArgumentList(
                                    TypeArgumentList(
                                        SeparatedList<TypeSyntax>(
                                            genericName.TypeArgumentList.Arguments.SelectAsEnumerable( _ => OmittedTypeArgument() ) ) ) ),
                            var x => throw new AssertionFailedException( $"Unsupported canonical generic instance syntax {x}." ),
                        },
                    _ => syntaxGenerationContext.SyntaxGenerator.Type( this._type.GetSymbol() ),
                };

            return TypeOfExpression( typeExpression );
        }

        protected override bool CanBeNull => false;

        public override IType Type => ((ICompilationInternal) this._type.Compilation).Factory.GetTypeByReflectionType( typeof(System.Type) );
    }
}