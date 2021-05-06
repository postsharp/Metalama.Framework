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

        public IMethod Method { get; }

        public IProperty Property => throw new InvalidOperationException();

        public IEvent Event => throw new InvalidOperationException();

        [Memo]
        public IAdviceParameterList Parameters => new AdviceParameterList( this.Method );

        public INamedType Type { get; }

        public ICompilation Compilation { get; }

        // TODO: when possible, this vanishes (e.g. `target.This.Property` is compiled to just `Property`); fix it so that it produces `this` or the name of the type, depending on whether the member on the right is static
        public dynamic This => new CurrentTypeOrInstanceDynamic( !this.Method.IsStatic, this.Type );

        public TemplateContextImpl( IMethod method, INamedType type, ICompilation compilation, IDiagnosticSink diagnosticSink )
        {
            this.Method = method;
            this.Type = type;
            this.Compilation = compilation;
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