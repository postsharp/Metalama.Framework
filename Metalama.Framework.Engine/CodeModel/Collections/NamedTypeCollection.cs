// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal sealed partial class NamedTypeCollection : MemberOrNamedTypeCollection<INamedType>, INamedTypeCollection
    {
        public NamedTypeCollection( INamespaceOrNamedType declaringType, IReadOnlyList<IRef<INamedType>> sourceItems, bool includeNestedTypes = false ) :
            base( declaringType, IncludeNestedTypes( declaringType.Compilation, sourceItems, includeNestedTypes ) ) { }

        public NamedTypeCollection( ICompilation declaringCompilation, IReadOnlyList<IRef<INamedType>> sourceItems, bool includeNestedTypes = false ) :
            base( declaringCompilation, IncludeNestedTypes( declaringCompilation, sourceItems, includeNestedTypes ) ) { }

        public IEnumerable<INamedType> OfTypeDefinition( INamedType typeDefinition )
        {
            var typeDefinitionRef = typeDefinition.ToRef();

            // Enumerate the source without causing a resolution of the reference.
            foreach ( var reference in this.Source )
            {
                if ( reference.IsConvertibleTo( typeDefinitionRef, ConversionKind.TypeDefinition ) )
                {
                    // Resolve the reference and store the declaration.
                    var member = this.GetItem( reference );

                    // Return the result.
                    yield return member;
                }
            }
        }

        private static IReadOnlyList<IRef<INamedType>> IncludeNestedTypes(
            ICompilation compilation,
            IReadOnlyList<IRef<INamedType>> sourceItems,
            bool includeNestedTypes )
        {
            if ( !includeNestedTypes )
            {
                return sourceItems;
            }
            else
            {
                return new FlattenedList( compilation.GetCompilationModel(), sourceItems );
            }
        }
    }
}