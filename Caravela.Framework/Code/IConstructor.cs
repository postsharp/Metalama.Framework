// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Project;
using System.Reflection;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents an instance constructor or a static constructor.
    /// </summary>
    public interface IConstructor : IMethodBase
    {
        [return: RunTimeOnly]
        ConstructorInfo ToConstructorInfo();
    }
}