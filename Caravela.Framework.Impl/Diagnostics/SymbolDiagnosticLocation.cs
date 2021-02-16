// unset

using Caravela.Framework.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace Caravela.Framework.Impl.Diagnostics
{
    internal class RoslynDiagnosticLocation : IDiagnosticLocation
    {
        public static RoslynDiagnosticLocation? ForSymbol( ISymbol? symbol )
        {
            if ( symbol == null )
            {
                return null;
            }

            var bestDeclaration = symbol.DeclaringSyntaxReferences
                .OrderByDescending( r => r.SyntaxTree.FilePath.Length )
                .FirstOrDefault();

            if ( bestDeclaration == null )
            {
                return null;
            }

            return new RoslynDiagnosticLocation( bestDeclaration.GetSyntax().GetLocation() );
        }
        
        public static RoslynDiagnosticLocation? ForAttribute( AttributeData? attribute )
        {
            if ( attribute == null )
            {
                return null;
            }

            var application = attribute.ApplicationSyntaxReference;

            if ( application == null )
            {
                return null;
            }

            return new RoslynDiagnosticLocation( application.GetSyntax().GetLocation() );
        }

        public RoslynDiagnosticLocation( Location location )
        {
            this.Location = location;
        }

        public Location Location { get; }
    }
}