// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.References;

namespace Caravela.Framework.Impl.ReflectionMocks
{
    internal interface ICompileTimeReflectionObject<out T> 
        where T : ICompilationElement
    {
        IDeclarationRef<T> Target { get; }
    }
}