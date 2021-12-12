using Metalama.Framework.Diagnostics;
using Metalama.Framework.Project;

namespace Metalama.Framework.Code;

internal interface ISyntaxReferenceService  : IService
{
    IDiagnosticLocation GetDiagnosticLocation( in SyntaxReference syntaxReference );

    string GetKind( in SyntaxReference syntaxReference );
}