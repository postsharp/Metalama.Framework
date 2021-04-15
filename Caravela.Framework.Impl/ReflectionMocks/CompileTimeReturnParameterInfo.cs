// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using System.Reflection;

namespace Caravela.Framework.Impl.ReflectionMocks
{
    internal class CompileTimeReturnParameterInfo : ParameterInfo
    {
        public CompileTimeReturnParameterInfo( MethodReturnParameter returnParameter )
        {
            this.Method = returnParameter.DeclaringMethod;
        }

        public Method Method { get; }
    }
}