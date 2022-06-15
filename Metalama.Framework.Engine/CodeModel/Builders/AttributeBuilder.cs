// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class AttributeBuilder : DeclarationBuilder, IAttribute, IObservableTransformation
    {
        private readonly IAttributeData _attributeConstruction;

        public AttributeBuilder( Advice advice, IDeclaration containingDeclaration, IAttributeData attributeConstruction ) : base( advice )
        {
            this._attributeConstruction = attributeConstruction;
            this.ContainingDeclaration = containingDeclaration;
        }

        string IDisplayable.ToDisplayString( CodeDisplayFormat? format, CodeDisplayContext? context ) => throw new NotImplementedException();

        public override bool CanBeInherited => false;

        public override IDeclaration ContainingDeclaration { get; }

        bool IObservableTransformation.IsDesignTime => false;

        DeclarationOrigin IDeclaration.Origin => DeclarationOrigin.Aspect;

        IDeclaration? IDeclaration.ContainingDeclaration => this.ContainingDeclaration;

        IAttributeCollection IDeclaration.Attributes => AttributeCollection.Empty;

        public override DeclarationKind DeclarationKind => DeclarationKind.Attribute;

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this._attributeConstruction.ToString();

        public INamedType Type => this.Constructor.DeclaringType;

        public IConstructor Constructor => this._attributeConstruction.Constructor;

        public ImmutableArray<TypedConstant> ConstructorArguments => this._attributeConstruction.ConstructorArguments;

        public ImmutableArray<KeyValuePair<string, TypedConstant>> NamedArguments => this._attributeConstruction.NamedArguments;

        IType IHasType.Type => this.Type;

        public override SyntaxTree? PrimarySyntaxTree => ((IDeclarationImpl) this.ContainingDeclaration).PrimarySyntaxTree;

        public FormattableString FormatPredecessor() => $"attribute of type '{this.Type}' on '{this.ContainingDeclaration}'";

        public AttributeSyntax GetSyntax( SyntaxGenerationContext generationContext )
        {
            return generationContext.SyntaxGenerator.Attribute( this._attributeConstruction );
        }
    }
}