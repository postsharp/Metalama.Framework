// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Caravela.Framework.Impl.Aspects
{
    /// <summary>
    /// Represents an <see cref="AspectInstance"/> that is materialized by a custom attribute.
    /// </summary>
    internal class AttributeAspectInstance : AspectInstance
    {
        private readonly IAttribute _attribute;
        private readonly CompileTimeProjectLoader _loader;

        public AttributeAspectInstance( IAspect aspect, IDeclaration target, AspectClass aspectClass, IAttribute attribute, CompileTimeProjectLoader loader ) :
            base( aspect, target, aspectClass, new AspectPredecessor( AspectPredecessorKind.Attribute, attribute ) )
        {
            this._attribute = attribute;
            this._loader = loader;
        }

        private AttributeAspectInstance(
            IAspect aspect,
            IDeclaration target,
            AspectClass aspectClass,
            IAttribute attribute,
            in AspectPredecessor aspectPredecessor,
            CompileTimeProjectLoader loader ) : base( aspect, target, aspectClass, aspectPredecessor )

        {
            this._attribute = attribute;
            this._loader = loader;
        }

        public override AttributeAspectInstance CreateDerivedInstance( IDeclaration target )
        {
            var attributeData = this._attribute.GetAttributeData();

            if ( !this._loader.AttributeDeserializer.TryCreateAttribute( attributeData, NullDiagnosticAdder.Instance, out var attributeInstance ) )
            {
                // This should not happen because we've already deserialized the same attribute once before.
                throw new AssertionFailedException();
            }

            return new AttributeAspectInstance(
                (IAspect) attributeInstance,
                target,
                (AspectClass) this.AspectClass,
                this._attribute,
                new AspectPredecessor( AspectPredecessorKind.Inherited, this ),
                this._loader );
        }

        public AttributeSyntax ToSyntax( SyntaxGenerationContext generationContext )
        {
            var constructorArguments = this._attribute.ConstructorArguments.Select(
                a => SyntaxFactory.AttributeArgument(
                    generationContext.SyntaxGenerator.AttributeValueExpression( a.Value, generationContext.ReflectionMapper ) ) );

            var namedArguments = this._attribute.NamedArguments.Select(
                a => SyntaxFactory.AttributeArgument(
                    SyntaxFactory.NameEquals( a.Key ),
                    null,
                    generationContext.SyntaxGenerator.AttributeValueExpression( a.Value, generationContext.ReflectionMapper ) ) );

            var attribute = SyntaxFactory.Attribute(
                (NameSyntax) generationContext.SyntaxGenerator.Type( this._attribute.Type.GetSymbol() ),
                SyntaxFactory.AttributeArgumentList( SyntaxFactory.SeparatedList( constructorArguments.Concat( namedArguments ) ) ) );

            return attribute;
        }
    }
}