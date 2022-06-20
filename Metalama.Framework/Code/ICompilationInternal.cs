// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Code
{
    internal interface ICompilationInternal : ICompilation
    {
        ICompilationHelpers Helpers { get; }

        /// <summary>
        /// Gets a service that allows to create type instances and compare them.
        /// </summary>
        IDeclarationFactory Factory { get; }
    }
}