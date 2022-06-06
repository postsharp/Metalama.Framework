// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection;
using RefKind = Metalama.Framework.Code.RefKind;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class ParameterBuilder : DeclarationBuilder, IParameterBuilder, IParameterImpl, IObservableTransformation
    {
        private string? _name;
        private TypedConstant _defaultValue;

        public RefKind RefKind { get; set; }

        public IType Type { get; set; }

        public string Name
        {
            get => this._name ?? throw new NotSupportedException( "Cannot get the name of a return parameter." );
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

        public override IDeclaration ContainingDeclaration => this.DeclaringMember;

        bool IObservableTransformation.IsDesignTime => true;

        public override DeclarationKind DeclarationKind => DeclarationKind.Parameter;

        public IMember DeclaringMember { get; }

        public ParameterInfo ToParameterInfo() => throw new NotImplementedException();

        public bool IsReturnParameter => this.Index < 0;

        public ParameterBuilder( Advice advice, IMember declaringMember, int index, string? name, IType type, RefKind refKind ) : base( advice )
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