// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel.InternalInterfaces;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Reflection;
using RefKind = Microsoft.CodeAnalysis.RefKind;
using TypedConstant = Caravela.Framework.Code.TypedConstant;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class ReturnParameter : IParameter, IHasDiagnosticLocation, IDeclarationInternal
    {
        protected abstract RefKind SymbolRefKind { get; }

        public Code.RefKind RefKind => this.SymbolRefKind.ToOurRefKind();

        public abstract IType ParameterType { get; }

        public string Name => throw new NotSupportedException( "Cannot get the name of a return parameter." );

        public int Index => -1;

        TypedConstant IParameter.DefaultValue => default;

        public bool IsParams => false;

        public abstract IMemberOrNamedType DeclaringMember { get; }

        public ParameterInfo ToParameterInfo() => CompileTimeReturnParameterInfo.Create( this );

        DeclarationOrigin IDeclaration.Origin => DeclarationOrigin.Source;

        public IDeclaration? ContainingDeclaration => this.DeclaringMember;

        public abstract IAttributeList Attributes { get; }

        public DeclarationKind DeclarationKind => DeclarationKind.Parameter;

        public ICompilation Compilation => this.ContainingDeclaration?.Compilation ?? throw new AssertionFailedException();

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();

        public abstract bool Equals( IDeclaration other );

        public IDiagnosticLocation? DiagnosticLocation => this.DeclaringMember.DiagnosticLocation;

        Location? IHasDiagnosticLocation.DiagnosticLocation => this.DeclaringMember.GetDiagnosticLocation();

        public abstract ISymbol? Symbol { get; }

        public abstract DeclarationRef<IDeclaration> ToRef();

        public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ((IDeclarationInternal) this.DeclaringMember).DeclaringSyntaxReferences;
    }
}