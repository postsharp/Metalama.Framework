using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;

namespace Caravela.Framework.Validation
{
    public readonly struct ValidateReferenceContext<T>
        where T : IDeclaration
    {
        
        public IDiagnosticSink Diagnostics { get; }
        public T ReferencedDeclaration { get; }
        public IDeclaration ReferencingDeclaration { get; }
        
        public INamedType ReferencingType { get; }
        public DeclarationReferenceKind ReferenceKind { get; }

        // Must be a lazy implementation.
        public IDiagnosticLocation DiagnosticLocation { get; }
    }
}