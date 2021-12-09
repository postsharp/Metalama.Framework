// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using System.Reflection;

namespace Metalama.Framework.Engine.ReflectionMocks
{
    internal class CompileTimeParameterInfo : ParameterInfo, ICompileTimeReflectionObject<IParameter>
    {
        public ISdkRef<IParameter> Target { get; }

        private CompileTimeParameterInfo( IParameter parameter )
        {
            this.Target = parameter.ToTypedRef();
        }

        public static ParameterInfo Create( IParameter parameter ) => new CompileTimeParameterInfo( parameter );
    }
}