using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Project;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.AspectOrdering
{
    internal interface IAspectOrderingSource
    {
        IEnumerable<AspectOrderSpecification> GetAspectOrderSpecification( );
    }

    internal class AttributeAspectOrderingSource : IAspectOrderingSource
    {
        private readonly CompilationModel _compilation;
        private readonly CompileTimeAssemblyLoader _loader;

        public AttributeAspectOrderingSource( CompilationModel compilation, CompileTimeAssemblyLoader loader )
        {
            this._compilation = compilation;
            this._loader = loader;
        }

        public IEnumerable<AspectOrderSpecification> GetAspectOrderSpecification( )
        {
            var attributeType = this._compilation.Factory.GetTypeByReflectionType( typeof(AspectOrderAttribute) ).GetSymbol();
            
            var roslynCompilation = this._compilation.RoslynCompilation;
            
            // Get compile-time level attributes of the current assembly and all referenced assemblies.
            var attributes =
                roslynCompilation.Assembly.Modules
                    .SelectMany( m => m.ReferencedAssemblySymbols )
                    .Concat( new[] {roslynCompilation.Assembly} )
                    .SelectMany( a => a.GetAttributes() )
                    .Where( a => a.AttributeClass == attributeType );

            return attributes.Select( attributeData =>
            {
                var attributeInstance = (AspectOrderAttribute) this._loader.CreateAttributeInstance( new Attribute(  attributeData, this._compilation, null ) );
                return new AspectOrderSpecification( attributeInstance, attributeData.ApplicationSyntaxReference?.GetSyntax()?.GetLocation() );
            } );
        }
    }
}