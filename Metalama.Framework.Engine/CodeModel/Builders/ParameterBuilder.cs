// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection;
using RefKind = Metalama.Framework.Code.RefKind;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class ParameterBuilder : DeclarationBuilder, IParameterBuilder, IParameterImpl
    {
        private string? _name;
        private TypedConstant _defaultValue;

        public RefKind RefKind { get; set; }

        public IType Type { get; set; }

        public string Name
        {
            get => this._name ?? "<return>";
            set
                => this._name = this._name != null
                    ? value ?? throw new NotSupportedException( "Cannot set the parameter name to null." )
                    : throw new NotSupportedException( "Cannot set the name of a return parameter." );
        }

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

        public IMember DeclaringMember { get; }

        public ParameterInfo ToParameterInfo() => throw new NotImplementedException();

        public bool IsReturnParameter => this.Index < 0;

        public ParameterBuilder( MemberBuilder declaringMember, int index, string? name, IType type, RefKind refKind ) : base( declaringMember.ParentAdvice )
        {
            this.DeclaringMember = declaringMember;
            this.Index = index;
            this._name = name;
            this.Type = type;
            this.RefKind = refKind;
        }

        // TODO: How to implement this?
        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this.Name;

        public override bool CanBeInherited => ((IDeclarationImpl) this.DeclaringMember).CanBeInherited;

        public override SyntaxTree? PrimarySyntaxTree => ((IDeclarationImpl) this.DeclaringMember).PrimarySyntaxTree;
    }
}