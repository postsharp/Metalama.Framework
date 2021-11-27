﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using RefKind = Microsoft.CodeAnalysis.RefKind;
using TypedConstant = Caravela.Framework.Code.TypedConstant;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class ReturnParameter : IParameter, IDeclarationImpl
    {
        protected abstract RefKind SymbolRefKind { get; }

        public Code.RefKind RefKind => this.SymbolRefKind.ToOurRefKind();

        public abstract IType Type { get; }

        public string Name => throw new NotSupportedException( "Cannot get the name of a return parameter." );

        public int Index => -1;

        TypedConstant IParameter.DefaultValue => default;

        public bool IsParams => false;

        public abstract IMemberOrNamedType DeclaringMember { get; }

        public ParameterInfo ToParameterInfo() => CompileTimeReturnParameterInfo.Create( this );

        public virtual bool IsReturnParameter => true;

        public IAssembly DeclaringAssembly => this.DeclaringMember.DeclaringAssembly;

        DeclarationOrigin IDeclaration.Origin => DeclarationOrigin.Source;

        public IDeclaration? ContainingDeclaration => this.DeclaringMember;

        public abstract IAttributeList Attributes { get; }

        public DeclarationKind DeclarationKind => DeclarationKind.Parameter;

        public ICompilation Compilation => this.ContainingDeclaration?.Compilation ?? throw new AssertionFailedException();

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.ContainingDeclaration!.ToDisplayString() + "@return";

        public abstract bool Equals( IDeclaration other );

        Location? IDiagnosticLocationImpl.DiagnosticLocation => this.DeclaringMember.GetDiagnosticLocation();

        public abstract ISymbol? Symbol { get; }

        public abstract Ref<IDeclaration> ToRef();

        public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ((IDeclarationImpl) this.DeclaringMember).DeclaringSyntaxReferences;

        public bool CanBeInherited => ((IDeclarationImpl) this.DeclaringMember).CanBeInherited;

        public abstract IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true );

        public abstract IDeclaration OriginalDefinition { get; }

        public override string ToString() => this.DeclaringMember + ":return";
    }
}