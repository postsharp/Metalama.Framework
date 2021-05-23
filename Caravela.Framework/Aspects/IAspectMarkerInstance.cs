using Caravela.Framework.Code;
using Caravela.Framework.Validation;

namespace Caravela.Framework.Aspects
{
    [InternalImplement]
    public interface IAspectMarkerInstance
    {
        IAspectMarker Marker { get; }
        IDeclaration MarkedDeclaration { get; }
    }
}