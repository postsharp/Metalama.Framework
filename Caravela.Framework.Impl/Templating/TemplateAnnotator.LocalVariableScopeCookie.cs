using Caravela.Framework.Impl.CompileTime;
using System;

namespace Caravela.Framework.Impl.Templating
{
    internal partial class TemplateAnnotator
    {
        /// <summary>
        /// Current scope for declaring local variables.
        /// Default is coerced to this scope. Mismatched computed scope results in build error.
        /// </summary>
        private SymbolDeclarationScope _requiredVariableDeclarationScope;

        private LocalVariableScopeCookie EnterLocalVariableScope( SymbolDeclarationScope scope ) => new LocalVariableScopeCookie( this, scope );

        private struct LocalVariableScopeCookie : IDisposable
        {
            private readonly TemplateAnnotator _parent;
            private readonly SymbolDeclarationScope _initialValue;

            public LocalVariableScopeCookie( TemplateAnnotator parent, SymbolDeclarationScope newValue )
            {
                this._parent = parent;
                this._initialValue = parent._requiredVariableDeclarationScope;
                parent._requiredVariableDeclarationScope = newValue;
            }

            public void Dispose()
            {
                this._parent._requiredVariableDeclarationScope = this._initialValue;
            }
        }
    }
}
