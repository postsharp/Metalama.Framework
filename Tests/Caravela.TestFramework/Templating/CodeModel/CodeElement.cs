using System;
using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using CompilationModel = Caravela.Framework.Impl.CodeModel.Symbolic.CompilationModel;
using IHasDiagnosticLocation = Caravela.Framework.Impl.CodeModel.Symbolic.IHasDiagnosticLocation;

namespace Caravela.TestFramework.Templating.CodeModel
{
    internal abstract class CodeElement : ICodeElement, IHasDiagnosticLocation
    {
        protected CodeElement( CompilationModel compilation )
        {
            this.Compilation = compilation;
        }

        internal CompilationModel Compilation { get; }

        ICompilation ICodeElement.Compilation => this.Compilation;

        public virtual ICodeElement? ContainingElement => throw new NotImplementedException();

        public IReadOnlyList<IAttribute> Attributes => throw new NotImplementedException();

        public abstract CodeElementKind ElementKind { get; }

        protected internal abstract ISymbol Symbol { get; }

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) =>
            this.Symbol.ToDisplayString();

        public bool Equals( ICodeElement? other ) =>
            other is CodeElement codeElement &&
            SymbolEqualityComparer.Default.Equals( this.Symbol, codeElement.Symbol );

        public Location? DiagnosticLocation => DiagnosticLocationHelper.GetDiagnosticLocation( this.Symbol );

        IDiagnosticLocation? IDiagnosticTarget.DiagnosticLocation => this.DiagnosticLocation?.ToDiagnosticLocation();
    }
}
