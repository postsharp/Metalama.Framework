// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using RefKind = Caravela.Framework.Code.RefKind;
using TypedConstant = Caravela.Framework.Code.TypedConstant;

namespace Caravela.Framework.Impl.CodeModel.Pseudo
{
    internal sealed class PseudoParameter : IParameter, IDeclarationImpl
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

        public IType Type { get; }

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

        public PseudoParameter( IMethod declaringAccessor, int index, IType type, string? name )
        {
            this.DeclaringAccessor = declaringAccessor;
            this.Index = index;
            this.Type = type;
            this._name = name;
        }

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();

        public ParameterInfo ToParameterInfo() => throw new NotImplementedException();

        public bool IsReturnParameter => this.Index < 0;

        ISymbol? ISdkDeclaration.Symbol => null;

        Ref<IDeclaration> IDeclarationImpl.ToRef() => throw new NotImplementedException();

        ImmutableArray<SyntaxReference> IDeclarationImpl.DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

        public bool CanBeInherited => ((IDeclarationImpl) this.DeclaringMember).CanBeInherited;

        public IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true ) => throw new NotImplementedException();

        public IDeclaration OriginalDefinition => throw new NotImplementedException();

        public IAssembly DeclaringAssembly => this.DeclaringMember.DeclaringAssembly;
    }
}