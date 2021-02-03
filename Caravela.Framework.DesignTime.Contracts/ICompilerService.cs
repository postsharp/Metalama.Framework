using System.Runtime.InteropServices;

namespace Caravela.Framework.DesignTime.Contracts
{
    /// <summary>
    /// Base interface for a implemented by the compiler part of the software (not the UI part) that
    /// can be returned synchronously.
    /// </summary>
    
    // The type identifier cannot be modified even during refactoring. 
   [Guid("32aeeb0f-92e3-4952-91c0-1477f791b309")]
    public interface ICompilerService
    {
        
    }
}