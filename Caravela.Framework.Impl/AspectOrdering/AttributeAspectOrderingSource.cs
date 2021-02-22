using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Project;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.AspectOrdering
{
    internal class AttributeAspectOrderingSource : IAspectOrderingSource
    {
        private readonly CompilationModel _compilation;
        
        public AttributeAspectOrderingSource( CompilationModel compilation )
        {
            this._compilation = compilation;
        }

        public IEnumerable<AspectOrderSpecification> GetAspectOrderSpecification()
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
                var attributeInstance = AttributeDeserializer.SystemTypesDeserializer.CreateAttribute<AspectOrderAttribute>( new Attribute( attributeData, this._compilation, null ) );
                return new AspectOrderSpecification( attributeInstance, attributeData.ApplicationSyntaxReference?.GetSyntax()?.GetLocation() );
            } );
        }
    }
}