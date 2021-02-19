using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.CodeModel.Links
{
    internal interface IMemberLink<out T> : ICodeElementLink<T>
        where T : IMember
    {
        string Name { get; }
    }
}