using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.Transformations
{
    /// <summary>
    /// Represents an introduction to the code model that should be observable by aspects running after the aspect that added the introduction. 
    /// </summary>
    internal interface IObservableTransformation 
    {
        ICodeElement ContainingElement { get; }
    }
}