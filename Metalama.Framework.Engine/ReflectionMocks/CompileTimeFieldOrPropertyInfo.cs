// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.RunTime;

namespace Metalama.Framework.Engine.ReflectionMocks
{
    internal class CompileTimeFieldOrPropertyInfo : FieldOrPropertyInfo
    {
        public IFieldOrProperty FieldOrProperty { get; }

        private CompileTimeFieldOrPropertyInfo( IFieldOrProperty fieldOrProperty )
        {
            this.FieldOrProperty = fieldOrProperty;
        }

        public static FieldOrPropertyInfo Create( IFieldOrProperty fieldOrProperty ) => new CompileTimeFieldOrPropertyInfo( fieldOrProperty );
    }
}