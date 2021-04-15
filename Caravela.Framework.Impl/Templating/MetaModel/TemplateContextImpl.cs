// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class TemplateContextImpl : ITemplateContextTarget
    {
        private readonly IDiagnosticSink _diagnosticSink;

        public IMethod Method { get; }

        [Memo]
        public IAdviceParameterList Parameters => new AdviceParameterList( this.Method );

        public IType Type { get; }

        public ICompilation Compilation { get; }

        // TODO: when possible, this vanishes (e.g. `target.This.Property` is compiled to just `Property`); fix it so that it produces `this` or the name of the type, depending on whether the member on the right is static
        public dynamic This => new CurrentTypeOrInstanceDynamic( !this.Method.IsStatic, this.Type );

        public TemplateContextImpl( IMethod method, IType type, ICompilation compilation, IDiagnosticSink diagnosticSink )
        {
            this.Method = method;
            this.Type = type;
            this.Compilation = compilation;
            this._diagnosticSink = diagnosticSink;
        }

        void IDiagnosticSink.ReportDiagnostic( Severity severity, IDiagnosticLocation location, string id, string formatMessage, params object[] args )
            => this._diagnosticSink.ReportDiagnostic( severity, location, id, formatMessage, args );

        void IDiagnosticSink.ReportDiagnostic( Severity severity, string id, string formatMessage, params object[] args )
            => this._diagnosticSink.ReportDiagnostic( severity, id, formatMessage, args );

        public void SuppressDiagnostic( string id, ICodeElement scope )
            => this._diagnosticSink.SuppressDiagnostic( id, scope );

        public void SuppressDiagnostic( string id )
            => this._diagnosticSink.SuppressDiagnostic( id );

    }
}
