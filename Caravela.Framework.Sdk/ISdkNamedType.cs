using Caravela.Framework.Code;

namespace Caravela.Framework.Sdk
{
    /// <summary>
    /// Extends the user-level <see cref="INamedType"/> interface with a <see cref="ISdkType.TypeSymbol"/> exposing the Roslyn <see cref="ITypeSymbol"/>. 
    /// </summary>
    public interface ISdkNamedType : INamedType, ISdkType
    {
        
    }
}