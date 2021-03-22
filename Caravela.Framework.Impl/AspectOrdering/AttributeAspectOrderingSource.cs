// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Microsoft.CodeAnalysis;

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
            var attributeType = this._compilation.Factory.GetTypeByReflectionType( typeof( AspectOrderAttribute ) ).GetSymbol();

            var roslynCompilation = this._compilation.RoslynCompilation;

            // Get compile-time level attributes of the current assembly and all referenced assemblies.
            var attributes =
                roslynCompilation.Assembly.Modules
                    .SelectMany( m => m.ReferencedAssemblySymbols )
                    .Concat( new[] { roslynCompilation.Assembly } )
                    .SelectMany( assembly => assembly.GetAttributes().Select( attribute => ( attribute, assembly ) ) )
                    .Where( a => SymbolEqualityComparer.Default.Equals( a.attribute.AttributeClass, attributeType ) );

            return attributes.Select( attribute =>
            {
                var attributeInstance = AttributeDeserializer.SystemTypesDeserializer.CreateAttribute<AspectOrderAttribute>( 
                    new Attribute( attribute.attribute, this._compilation, this._compilation.Factory.GetAssembly( attribute.assembly ) ) );
                return new AspectOrderSpecification( attributeInstance, attribute.attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() );
            } );
        }
    }
}