// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.AspectOrdering
{
    internal class AttributeAspectOrderingSource : IAspectOrderingSource
    {
        private readonly Compilation _compilation;
        private readonly AttributeDeserializer _attributeDeserializer;

        public AttributeAspectOrderingSource( Compilation compilation, CompileTimeProjectLoader loader )
        {
            this._compilation = compilation;
            this._attributeDeserializer = loader.AttributeDeserializer;
        }

        public IEnumerable<AspectOrderSpecification> GetAspectOrderSpecification( IDiagnosticAdder diagnosticAdder )
        {
            var roslynCompilation = this._compilation;

            // Get compile-time level attributes of the current assembly and all referenced assemblies.
            var orderAttributeName = typeof(AspectOrderAttribute).FullName;

            var attributes =
                roslynCompilation.Assembly.Modules
                    .SelectMany( m => m.ReferencedAssemblySymbols )
                    .Concat( new[] { roslynCompilation.Assembly } )
                    .SelectMany( assembly => assembly.GetAttributes().Select( attribute => (attribute, assembly) ) )
                    .Where( a => a.attribute.AttributeClass?.GetReflectionName() == orderAttributeName );

            return attributes.Select(
                    attribute =>
                    {
                        if ( this._attributeDeserializer.TryCreateAttribute<AspectOrderAttribute>(
                            attribute.attribute,
                            diagnosticAdder,
                            out var attributeInstance ) )
                        {
                            return new AspectOrderSpecification( attributeInstance, attribute.attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() );
                        }
                        else
                        {
                            return null;
                        }
                    } )
                .WhereNotNull();
        }
    }
}