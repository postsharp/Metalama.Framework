using Caravela.Framework.Code;
using System.Collections.Generic;

namespace Caravela.Framework.Validation
{
    public interface IDeclarationReferenceValidator<T>
        where T : IDeclaration
    {
        void Initialize( IReadOnlyDictionary<string, string> properties );
        void ValidateReference( in ValidateReferenceContext<T> reference );
    }
}