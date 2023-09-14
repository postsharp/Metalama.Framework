// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Options;

namespace Metalama.Framework.Code
{
    internal interface ICompilationInternal : ICompilation
    {
        ICompilationHelpers Helpers { get; }

        /// <summary>
        /// Gets a service that allows to create type instances and compare them.
        /// </summary>
        IDeclarationFactory Factory { get; }

        IAspectRepository AspectRepository { get; }

        IAspectOptionsManager AspectOptionsManager { get; }
    }
}