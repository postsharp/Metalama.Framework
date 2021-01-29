using Caravela.Framework.Code;
using Caravela.Framework.Sdk;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel
{
    abstract class CodeElement : ICodeElement, IToSyntax
    {
        internal abstract SourceCompilation Compilation { get; }
        internal SymbolMap SymbolMap => this.Compilation.SymbolMap;

        public abstract ICodeElement? ContainingElement { get; }

        public abstract IReactiveCollection<IAttribute> Attributes { get; }

        public abstract CodeElementKind Kind { get; }

        protected internal abstract ISymbol Symbol { get; }

        protected static void CheckArguments( ICodeElement target, IImmutableList<IParameter> parameters, object[] arguments )
        {
            // TODO: somehow provide locations for the diagnostics?
            if ( parameters.LastOrDefault()?.IsParams == true )
            {
                // all non-params arguments have to be set + any number of params arguments
                int requiredArguments = parameters.Count - 1;
                if ( arguments.Length < requiredArguments )
                    throw new CaravelaException( GeneralDiagnosticDescriptors.MethodRequiresAtLeastNArguments, target, requiredArguments );
            }
            else
            {
                if ( arguments.Length != parameters.Count )
                    throw new CaravelaException( GeneralDiagnosticDescriptors.MethodRequiresNArguments, target, parameters.Count );
            }
        }

        private IEnumerable<CSharpSyntaxNode> ToSyntaxNodes() => this.Symbol.DeclaringSyntaxReferences.Select(r => (CSharpSyntaxNode)r.GetSyntax());
        // TODO: special case partial methods?
        CSharpSyntaxNode IToSyntax.GetSyntaxNode() => this.ToSyntaxNodes().Single();
        IEnumerable<CSharpSyntaxNode> IToSyntax.GetSyntaxNodes() => this.ToSyntaxNodes();

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext context = null ) =>
            this.Symbol.ToDisplayString();
    }
}
