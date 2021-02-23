using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class CodeElementEqualityComparer : IEqualityComparer<ICodeElement>
    {
        private readonly CodeElementLinkEqualityComparer<CodeElementLink<ICodeElement>> _innerComparer = CodeElementLinkEqualityComparer<CodeElementLink<ICodeElement>>.Instance;

        private CodeElementEqualityComparer()
        {
        }

        public static IEqualityComparer<ICodeElement> Instance { get; } = new CodeElementEqualityComparer();

        public bool Equals( ICodeElement x, ICodeElement y ) => this._innerComparer.Equals( x.ToLink(), y.ToLink() );

        public int GetHashCode( ICodeElement obj ) => this._innerComparer.GetHashCode( obj.ToLink() );
    }
}
