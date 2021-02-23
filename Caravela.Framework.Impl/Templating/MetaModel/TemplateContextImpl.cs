using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class TemplateContextImpl : ITemplateContext
    {
        private IDiagnosticSink _diagnosticSink;

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

        void IDiagnosticSink.ReportDiagnostic( Severity severity, IDiagnosticLocation? location, string id, string formatMessage, params object[] args ) => this._diagnosticSink.ReportDiagnostic( severity, location, id, formatMessage, args );

        void IDiagnosticSink.ReportDiagnostic( Severity severity, string id, string formatMessage, params object[] args ) => this._diagnosticSink.ReportDiagnostic( severity, id, formatMessage, args );
    }

    internal class CurrentTypeOrInstanceDynamic : IDynamicMemberDifferentiated
    {
        private readonly bool _allowExpression;
        private readonly IType _type;

        public CurrentTypeOrInstanceDynamic( bool allowExpression, IType type )
        {
            this._allowExpression = allowExpression;
            this._type = type;
        }

        public RuntimeExpression CreateExpression()
        {
            if ( this._allowExpression )
            {
                return new( ThisExpression(), this._type, false );
            }

            // TODO: diagnostic
            throw new InvalidOperationException( "Can't directly access 'this' on a static method." );
        }

        RuntimeExpression IDynamicMemberDifferentiated.CreateMemberAccessExpression( string member ) => new( IdentifierName( Identifier( member ) ) );
    }
}
