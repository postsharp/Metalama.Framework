// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Impl.Impl.CodeModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Reflection;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal sealed class ParameterBuilder : DeclarationBuilder, IParameterBuilder, IParameterInternal
    {
        private readonly string? _name;
        private TypedConstant _defaultValue;

        public RefKind RefKind { get; set; }

        public IType ParameterType { get; set; }

        public string Name => this._name ?? throw new NotSupportedException( "Cannot get the name of a return parameter." );

        public int Index { get; }

        public TypedConstant DefaultValue
        {
            get => this._defaultValue;
            set
                => this._defaultValue = this._name != null
                    ? value
                    : throw new NotSupportedException( "Cannot set default value of a return parameter." );
        }

        public bool IsParams { get; set; }

        public override IDeclaration? ContainingDeclaration => this.DeclaringMember;

        public override DeclarationKind DeclarationKind => DeclarationKind.Parameter;

        public IMemberOrNamedType DeclaringMember { get; }

        public ParameterInfo ToParameterInfo() => throw new NotImplementedException();

        public ParameterBuilder( MemberOrNamedTypeBuilder declaringMember, int index, string? name, IType type, RefKind refKind ) : base(
            declaringMember.ParentAdvice )
        {
            this.DeclaringMember = declaringMember;
            this.Index = index;
            this._name = name;
            this.ParameterType = type;
            this.RefKind = refKind;
        }

        // TODO: How to implement this?
        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        {
            return this.Name;
        }

        internal ParameterSyntax ToDeclarationSyntax()
        {
            var syntaxGenerator = LanguageServiceFactory.CSharpSyntaxGenerator;

            return syntaxGenerator.ParameterDeclaration(
                this.Name,
                syntaxGenerator.TypeExpression( this.ParameterType.GetSymbol() ),
                this.DefaultValue.ToExpressionSyntax( this.Compilation ),
                this.RefKind.ToRoslynRefKind() );
        }
    }
}