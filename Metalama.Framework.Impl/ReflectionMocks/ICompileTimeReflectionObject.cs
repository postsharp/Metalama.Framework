// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Impl.CodeModel.References;

namespace Metalama.Framework.Impl.ReflectionMocks
{
    internal interface ICompileTimeReflectionObject<out T>
        where T : class, ICompilationElement
    {
        ISdkRef<T> Target { get; }
    }
}