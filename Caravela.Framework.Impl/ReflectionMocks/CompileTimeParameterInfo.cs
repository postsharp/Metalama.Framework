// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.References;
using System.Reflection;

namespace Caravela.Framework.Impl.ReflectionMocks
{
    internal class CompileTimeParameterInfo : ParameterInfo, ICompileTimeReflectionObject<IParameter>
    {
        public ISdkRef<IParameter> Target { get; }

        private CompileTimeParameterInfo( IParameter parameter )
        {
            this.Target = parameter.ToRef();
        }

        public static ParameterInfo Create( IParameter parameter ) => new CompileTimeParameterInfo( parameter );
    }
}