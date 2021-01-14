using Caravela.Framework.Project;
using Caravela.Reactive;

namespace Caravela.Framework.Code
{
    [CompileTime]
    public interface ICodeElement : IDisplayable
    {
        ICodeElement? ContainingElement { get; }
        IReactiveCollection<IAttribute> Attributes { get; }

        public CodeElementKind Kind { get; }
    }
}