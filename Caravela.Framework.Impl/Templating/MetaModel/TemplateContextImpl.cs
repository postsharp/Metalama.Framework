// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using System;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class TemplateContextImpl : ITemplateContext
    {
        private readonly IDiagnosticSink _diagnosticSink;

        public IMethod CurrentMethod { get; }

        public IProperty CurrentProperty => throw new InvalidOperationException();

        public IEvent CurrentEvent => throw new InvalidOperationException();

        [Memo]
        public IAdviceParameterList CurrentParameters => new AdviceParameterList( this.CurrentMethod );

        public INamedType CurrentType { get; }

        public ICompilation CurrentCompilation { get; }

        // TODO: when possible, this vanishes (e.g. `target.This.Property` is compiled to just `Property`); fix it so that it produces `this` or the name of the type, depending on whether the member on the right is static
        public dynamic This => new CurrentTypeOrInstanceDynamic( !this.CurrentMethod.IsStatic, this.CurrentType );

        public TemplateContextImpl( IMethod method, INamedType type, ICompilation compilation, IDiagnosticSink diagnosticSink )
        {
            this.CurrentMethod = method;
            this.CurrentType = type;
            this.CurrentCompilation = compilation;
            this._diagnosticSink = diagnosticSink;
        }

        void IDiagnosticSink.Report( Severity severity, IDiagnosticLocation location, string id, string formatMessage, params object[] args )
            => this._diagnosticSink.Report( severity, location, id, formatMessage, args );

        void IDiagnosticSink.Report( Severity severity, string id, string formatMessage, params object[] args )
            => this._diagnosticSink.Report( severity, id, formatMessage, args );

        public void Suppress( string id, ICodeElement scope ) => this._diagnosticSink.Suppress( id, scope );

        public void Suppress( string id ) => this._diagnosticSink.Suppress( id );
    }
}