// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code
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