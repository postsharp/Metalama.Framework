// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using System.Reflection;

namespace Metalama.Framework.Engine.ReflectionMocks
{
    internal sealed class CompileTimeReturnParameterInfo : ParameterInfo, ICompileTimeReflectionObject<IParameter>
    {
        public ISdkRef<IParameter> Target { get; }

        private CompileTimeReturnParameterInfo( IParameter returnParameter )
        {
            this.Target = returnParameter.ToTypedRef();
        }

        public static ParameterInfo Create( IParameter returnParameter )
        {
            return new CompileTimeReturnParameterInfo( returnParameter );
        }
    }
}