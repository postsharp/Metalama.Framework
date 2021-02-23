using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Links;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class CodeElement : ICodeElementInternal, IHasDiagnosticLocation
    {
        protected CodeElement( CompilationModel compilation )
        {
            this.Compilation = compilation;
        }

        internal CompilationModel Compilation { get; }

        ICompilation ICodeElement.Compilation => this.Compilation;

        CodeOrigin ICodeElement.Origin => CodeOrigin.Source;

        [Memo]
        public virtual ICodeElement? ContainingElement => this.Compilation.Factory.GetCodeElement( this.Symbol.ContainingSymbol );

        [Memo]
        public virtual IAttributeList Attributes =>
            new AttributeList(
                this.Symbol!.GetAttributes()
                .Select( a => new AttributeLink( a, CodeElementLink.FromSymbol<ICodeElement>( this.Symbol ) ) ),
                this.Compilation );

        public abstract CodeElementKind ElementKind { get; }

        public abstract ISymbol Symbol { get; }

        public virtual CodeElementLink<ICodeElement> ToLink() => CodeElementLink.FromSymbol( this.Symbol );

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) =>
            this.Symbol.ToDisplayString();

        public Location? DiagnosticLocation => DiagnosticLocationHelper.GetDiagnosticLocation( this.Symbol );

        IDiagnosticLocation? IDiagnosticTarget.DiagnosticLocation => this.DiagnosticLocation?.ToDiagnosticLocation();
    }
}
