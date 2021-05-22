using Caravela.Framework.ArchitectureValidation;
using Caravela.Framework.Code;

namespace Caravela.Framework.Aspects
{
    [InternalImplement]
    public interface IAspectMarkerInstance
    {
        IAspectMarker Marker { get; }
        IDeclaration MarkedDeclaration { get; }
    }
}