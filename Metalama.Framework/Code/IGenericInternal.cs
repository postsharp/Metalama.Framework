// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Code
{
    internal interface IGenericInternal : IDeclarationInternal, IGeneric
    {
        /// <summary>
        /// Constructs a generic instance of the current generic type definition.
        /// </summary>
        /// <param name="typeArguments"></param>
        /// <returns></returns>
        IGeneric ConstructGenericInstance( params IType[] typeArguments );
    }
}