// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel.Links;
using Microsoft.CodeAnalysis;
using RefKind = Caravela.Framework.Code.RefKind;
using TypedConstant = Caravela.Framework.Code.TypedConstant;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class ReturnParameter : IParameter, IHasDiagnosticLocation, ICodeElementInternal
    {

        protected abstract Microsoft.CodeAnalysis.RefKind SymbolRefKind { get; }

        public RefKind RefKind => this.SymbolRefKind.ToOurRefKind();

        public abstract IType ParameterType { get; }

        public string Name => throw new NotSupportedException( "Cannot get the name of a return parameter." );

        public int Index => -1;

        TypedConstant IParameter.DefaultValue => default;

        public bool IsParams => false;

        public abstract IMember DeclaringMember { get; }

        CodeOrigin ICodeElement.Origin => CodeOrigin.Source;

        public ICodeElement? ContainingElement => this.DeclaringMember;

        public abstract IAttributeList Attributes { get; }

        public CodeElementKind ElementKind => CodeElementKind.Parameter;

        public ICompilation Compilation => this.ContainingElement?.Compilation ?? throw new AssertionFailedException();

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();

        public abstract bool Equals( ICodeElement other );

        public IDiagnosticLocation? LocationForDiagnosticReport => this.DeclaringMember.LocationForDiagnosticReport;

        Location? IHasDiagnosticLocation.LocationForDiagnosticReport => this.DeclaringMember.GetLocationForDiagnosticReport();

        public abstract ISymbol? Symbol { get; }

        public abstract CodeElementLink<ICodeElement> ToLink();
    }
}
