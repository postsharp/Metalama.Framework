// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class AdviceParameter : IAdviceParameter
    {
        private readonly IParameter _parameter;

        public AdviceParameter( IParameter p )
        {
            this._parameter = p;
        }

        public RefKind RefKind => this._parameter.RefKind;

        public TypedConstant DefaultValue => this._parameter.DefaultValue;

        public bool IsParams => this._parameter.IsParams;

        public IMember DeclaringMember => this._parameter.DeclaringMember;

        public ParameterInfo ToParameterInfo() => this._parameter.ToParameterInfo();

        public IType ParameterType => this._parameter.ParameterType;

        public string Name => this._parameter.Name.AssertNotNull();

        public int Index => this._parameter.Index;

        DeclarationOrigin IDeclaration.Origin => this._parameter.Origin;

        public IDeclaration? ContainingDeclaration => this._parameter.ContainingDeclaration;

        public IAttributeList Attributes => this._parameter.Attributes;

        public DeclarationKind DeclarationKind => this._parameter.DeclarationKind;

        public ICompilation Compilation => this._parameter.Compilation;

        public dynamic Value
        {
            get => new DynamicExpression( SyntaxFactory.IdentifierName( this._parameter.Name! ), this._parameter.ParameterType, true );
            set => throw new NotImplementedException();
        }

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this._parameter.ToDisplayString( format, context );

        public bool Equals( IDeclaration other ) => throw new NotImplementedException();

        public IDiagnosticLocation? DiagnosticLocation => this._parameter.DiagnosticLocation;
    }
}