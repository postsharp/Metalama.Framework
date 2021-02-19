using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.CodeModel.Links
{
    internal interface ICodeElementLink
    {
        object? LinkedObject { get; }
    }
        
    internal interface ICodeElementLink<out T> : ICodeElementLink
        where T : ICodeElement
    {
        T GetForCompilation( CompilationModel compilation );
    }
}