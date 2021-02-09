using System.Collections.Generic;
using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class Attribute : IAttribute
    {
        INamedType IAttribute.Type => this.Type;

        public abstract NamedType Type { get; }

        IMethod IAttribute.Constructor => this.Constructor;

        public abstract Method Constructor { get; }

        public abstract IReadOnlyList<object?> ConstructorArguments { get; }

        public abstract IReadOnlyDictionary<string, object?> NamedArguments { get; }
    }
}
