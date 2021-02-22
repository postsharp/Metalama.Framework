using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Sdk;

namespace Caravela.Framework.Impl
{
    internal interface IAspectSource
    {
        AspectSourcePriority Priority { get; }
        
        IEnumerable<INamedType> AspectTypes { get; }
        IEnumerable<ICodeElement> GetExclusions( INamedType aspectType );

        /// <summary>
        /// Returns a set of <see cref="AspectInstance"/> of a given type. This method is called when the given aspect
        /// type is being processed, not before.
        /// </summary>
        IEnumerable<AspectInstance> GetAspectInstances( INamedType aspectType );
    }
}