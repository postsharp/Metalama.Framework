// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using System.Reflection;

namespace Caravela.Framework.Impl.ReflectionMocks
{
    internal class CompileTimeReturnParameterInfo : ParameterInfo
    {
        private CompileTimeReturnParameterInfo( ReturnParameter returnParameter )
        {
            this.DeclaringMember = returnParameter.DeclaringMember;
        }

        public static ParameterInfo Create( ReturnParameter returnParameter )
        {
            return new CompileTimeReturnParameterInfo( returnParameter );
        }

        public IMember DeclaringMember { get; }
    }
}