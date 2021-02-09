using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Sdk;
using Caravela.Reactive;
using Caravela.Reactive.Sources;
using System.Collections.Generic;

namespace Caravela.Framework.Impl
{
    internal class AttributeAspectSource 
    {
        private readonly ICompilation _compilation;
        private readonly CompileTimeAssemblyLoader _loader;

        public AttributeAspectSource( ICompilation compilation, CompileTimeAssemblyLoader loader )
        {
            this._compilation = compilation;
            this._loader = loader;
        }

        public IReadOnlyList<AspectInstance> GetAspects()
        {
            var iAspect = this._compilation.GetTypeByReflectionType( typeof( IAspect ) )!;

            var codeElements = new ICodeElement[] { this._compilation }.ToImmutableReactive().SelectDescendants( codeElement => codeElement switch
            {
                ICompilation compilation => compilation.DeclaredTypes,
                INamedType namedType => namedType.NestedTypes.Union<ICodeElement>( namedType.Methods ).Union( namedType.Properties ).Union( namedType.Events ),
                IMethod method => method.LocalFunctions.ToImmutableReactive(),
                _ => ImmutableReactiveCollection<ICodeElement>.Empty
            } );

            return from codeElement in codeElements
                   from attribute in codeElement.Attributes
                   where attribute.Type.Is( iAspect )
                   let aspect = (IAspect) this._loader.CreateAttributeInstance( attribute )
                   select new AspectInstance( aspect, codeElement, attribute.Type );
        }
    }
}
