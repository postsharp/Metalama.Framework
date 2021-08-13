// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Reflection;
using RefKind = Caravela.Framework.Code.RefKind;
using TypedConstant = Caravela.Framework.Code.TypedConstant;

namespace Caravela.Framework.Impl.CodeModel.Pseudo
{
    internal sealed class PseudoParameter : IParameter, IDeclarationInternal
    {
        private readonly string? _name;

        private IMethod DeclaringAccessor { get; }

        public IMemberOrNamedType DeclaringMember => this.DeclaringAccessor;

        public RefKind RefKind
            => this.DeclaringAccessor.ContainingDeclaration switch
            {
                Property property => property.RefKind,
                Field _ => RefKind.None,
                Event _ => RefKind.None,
                _ => throw new AssertionFailedException()
            };

        public IType ParameterType { get; }

        public string Name => this._name ?? throw new NotSupportedException( "Cannot get the name of a return parameter." );

        public int Index { get; }

        public TypedConstant DefaultValue => default;

        public bool IsParams => false;

        public DeclarationOrigin Origin => DeclarationOrigin.Source;

        public IDeclaration? ContainingDeclaration => this.DeclaringAccessor;

        public IAttributeList Attributes => AttributeList.Empty;

        public DeclarationKind DeclarationKind => DeclarationKind.Parameter;

        public IDiagnosticLocation? DiagnosticLocation => null;

        public ICompilation Compilation => this.DeclaringAccessor.Compilation;

        public PseudoParameter( IMethod declaringAccessor, int index, IType parameterType, string? name )
        {
            this.DeclaringAccessor = declaringAccessor;
            this.Index = index;
            this.ParameterType = parameterType;
            this._name = name;
        }

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();

        public ParameterInfo ToParameterInfo() => throw new NotImplementedException();

        ISymbol? ISdkDeclaration.Symbol => null;

        DeclarationRef<IDeclaration> IDeclarationInternal.ToRef() => throw new NotImplementedException();

        ImmutableArray<SyntaxReference> IDeclarationInternal.DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;
    }
}