using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    /// <summary>
    /// Contains information about an expression that is passed to dynamic methods.
    /// </summary>
    public readonly struct RuntimeExpression
    {
        public ExpressionSyntax Syntax { get; }

        public bool IsNull { get; }

        private readonly string? _typeDocumentationCommentId;
        private readonly ITypeSymbol? _typeSymbol;

        public RuntimeExpression( ExpressionSyntax syntax, string typeDocumentationCommentId )
        {
            this.Syntax = syntax;
            this.IsNull = false;
            this._typeDocumentationCommentId = typeDocumentationCommentId;
            this._typeSymbol = null;
        }

        private RuntimeExpression( ExpressionSyntax syntax, ITypeSymbol? typeSymbol )
        {
            this.Syntax = syntax;
            this.IsNull = false;
            this._typeDocumentationCommentId = null;
            this._typeSymbol = typeSymbol;
        }

        public RuntimeExpression( ExpressionSyntax syntax, IType? type = null ) : this( syntax, type?.GetSymbol() ) { }

        public RuntimeExpression( ExpressionSyntax syntax, bool isNull )
        {
            this.Syntax = syntax;
            this.IsNull = isNull;
            this._typeDocumentationCommentId = null;
            this._typeSymbol = null;
        }

        internal ITypeSymbol? GetTypeSymbol( SourceCompilation compilation )
        {
            if ( this._typeSymbol != null )
                return this._typeSymbol;

            if ( this._typeDocumentationCommentId != null )
                return DocumentationCommentId.GetFirstSymbolForReferenceId( this._typeDocumentationCommentId, compilation.RoslynCompilation ) as ITypeSymbol;

            return null;
        }

        public static implicit operator ExpressionSyntax(RuntimeExpression runtimeExpression) => runtimeExpression.Syntax;
    }
}
