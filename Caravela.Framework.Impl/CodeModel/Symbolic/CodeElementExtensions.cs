using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Collections;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    internal static class CodeElementExtensions
    {
        /// <summary>
        /// Select all code elements recursively contained in a given code element (i.e. all children of the tree).
        /// </summary>
        /// <param name="codeElement"></param>
        /// <returns></returns>
        public static IEnumerable<ICodeElement> SelectContainedElements( this ICodeElement codeElement ) =>
            new[] { codeElement }.SelectDescendants(
                codeElement => codeElement switch
                {
                    ICompilation compilation => compilation.DeclaredTypes,
                    INamedType namedType => namedType.NestedTypes
                        .Concat<ICodeElement>( namedType.Methods )
                        .Concat( namedType.Properties )
                        .Concat( namedType.Events ),
                    IMethod method => method.LocalFunctions
                        .Concat<ICodeElement>( method.Parameters )
                        .Concat( method.GenericParameters )
                        .ConcatNotNull( method.ReturnParameter ),
                    _ => null
                } );

        public static Location? GetLocation( this ICodeElement codeElement )
            => codeElement switch
            {
                IHasLocation hasLocation => hasLocation.Location,
                _ => null
            };
    }
}