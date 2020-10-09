using System.Collections.Generic;

namespace PostSharp.Framework.Impl
{
    abstract class CodeElement : ICodeElement
    {
        internal abstract Compilation Compilation { get; }
        internal Cache Cache => Compilation.Cache;

        public abstract ICodeElement? ContainingElement { get; }
        public abstract IReadOnlyList<IAttribute> Attributes { get; }
    }
}
