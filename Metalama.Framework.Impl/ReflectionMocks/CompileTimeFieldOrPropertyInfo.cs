// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.RunTime;

namespace Metalama.Framework.Impl.ReflectionMocks
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