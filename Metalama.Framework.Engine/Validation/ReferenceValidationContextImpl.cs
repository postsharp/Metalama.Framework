// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Validation;

internal sealed class ReferenceValidationContextImpl : ReferenceValidationContext
{
    private readonly ReferenceValidatorInstance _parent;
    private readonly IEnumerable<ReferencingSymbolInfo> _references;

    public ReferenceValidationContextImpl(
        ReferenceValidatorInstance parent,
        IDeclaration referencedDeclaration,
        IDeclaration referencingDeclaration,
        IAspectState? aspectState,
        IDiagnosticSink diagnosticSink,
        IEnumerable<ReferencingSymbolInfo> references ) : base( referencedDeclaration, referencingDeclaration, parent.Granularity, aspectState, diagnosticSink )
    {
        this._parent = parent;
        this._references = references;
    }

    [Memo]
    public override IEnumerable<ReferenceDetail> Details
        => this._references.SelectMany(
                r => r.Nodes
                    .Where( n => (n.ReferenceKind & this._parent.Properties.ReferenceKinds) != 0 )
                    .Select(
                        n => new ReferenceDetail(
                            this,
                            (object?) n.Syntax.AsNode() ?? n.Syntax.AsToken(),
                            r.ReferencedSymbol,
                            r.ReferencingSymbol,
                            n.ReferenceKind ) ) )
            .Cache();

    internal override IDiagnosticSource DiagnosticSource => this._parent;

    [Memo]
    [Obsolete]
    public override ReferenceKinds ReferenceKinds => this.Details.First().ReferenceKind;

    internal override ISourceReferenceImpl SourceReferenceImpl => CodeModel.SourceReferenceImpl.Instance;

    internal override IDeclaration ResolveOriginDeclaration( ReferenceDetail referenceDetail )
        => this.Compilation.GetCompilationModel().Factory.GetDeclaration( (ISymbol) referenceDetail.OriginSymbol );

    internal override IDeclaration ResolveDestinationDeclaration( ReferenceDetail referenceDetail )
        => this.Compilation.GetCompilationModel().Factory.GetDeclaration( (ISymbol) referenceDetail.DestinationSymbol );

    internal override IDiagnosticLocation? ResolveDiagnosticLocation( ReferenceDetail referenceDetail )
        => referenceDetail.NodeOrToken switch
        {
            SyntaxNode node => node.GetDiagnosticLocation().ToDiagnosticLocation(),
            SyntaxToken token => token.GetLocation().ToDiagnosticLocation(),
            _ => null
        };
}