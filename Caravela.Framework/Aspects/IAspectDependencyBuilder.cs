using Caravela.Framework.ArchitectureValidation;
using System;

namespace Caravela.Framework.Aspects
{
    [InternalImplement]
    public interface IAspectDependencyBuilder
    {
        void IsExecutedAfter<TAspect>()
            where TAspect : IAspect;

        void ConflictsWith<TAspect>()
            where TAspect : IAspect;

        void RequiresAspect<TAspect>()
            where TAspect : Attribute, IAspect, new();
    }
}