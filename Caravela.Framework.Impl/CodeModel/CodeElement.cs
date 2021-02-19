using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Links;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel
{
    
    internal abstract class CodeElement : ICodeElement, IHasDiagnosticLocation, ICodeElementLink<ICodeElement>
    {
        protected CodeElement( CompilationModel compilation )
        {
            this.Compilation = compilation;
        }

        internal CompilationModel Compilation { get; }

        ICompilation ICodeElement.Compilation => this.Compilation;

        [Memo]
        public virtual ICodeElement? ContainingElement => this.Compilation.Factory.GetCodeElement( this.Symbol.ContainingSymbol );

        [Memo]
        public IAttributeList Attributes =>
            new AttributeList( 
                this.Symbol.GetAttributes()
                .Select( a => new AttributeLink(a, CodeElementLink.FromSymbol<ICodeElement>(this.Symbol)) ),
                this.Compilation );

        public abstract CodeElementKind ElementKind { get; }

        protected internal abstract ISymbol Symbol { get; }

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) =>
            this.Symbol.ToDisplayString();

        public Location? DiagnosticLocation => DiagnosticLocationHelper.GetDiagnosticLocation( this.Symbol );

        IDiagnosticLocation? IDiagnosticTarget.DiagnosticLocation => this.DiagnosticLocation?.ToDiagnosticLocation();

        protected T GetForCompilation<T>( CompilationModel compilation )
            where T : ICodeElement 
            => compilation == this.Compilation ? (T) (object) this : throw new AssertionFailedException();

        ICodeElement ICodeElementLink<ICodeElement>.GetForCompilation( CompilationModel compilation ) =>
            this.GetForCompilation<ICodeElement>( compilation );
            

        object? ICodeElementLink.Target => this.Symbol;
   }
}
