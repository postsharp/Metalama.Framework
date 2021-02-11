// unset

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    internal interface IHasLocation
    {
        /// <summary>
        /// Gets the location of the code element, to emit diagnostics.
        /// </summary>
        Location? Location { get; }
    }
    
    

    internal static class CodeElementExtensions
    {
        public static Location? GetLocation( this ICodeElement codeElement )
            => codeElement switch
            {
                IHasLocation hasLocation => hasLocation.Location,
                _ => null
            };
    }
    
    
}