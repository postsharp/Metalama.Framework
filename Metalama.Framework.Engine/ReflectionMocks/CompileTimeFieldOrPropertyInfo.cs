// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.RunTime;

namespace Metalama.Framework.Engine.ReflectionMocks
{
    internal class CompileTimeFieldOrPropertyInfo : FieldOrPropertyInfo
    {
        public IFieldOrPropertyOrIndexer FieldOrPropertyIndexer { get; }

        private CompileTimeFieldOrPropertyInfo( IFieldOrPropertyOrIndexer fieldOrPropertyOrIndexer )
        {
            this.FieldOrPropertyIndexer = fieldOrPropertyOrIndexer;
        }

        public static FieldOrPropertyInfo Create( IFieldOrPropertyOrIndexer fieldOrPropertyOrIndexer )
            => new CompileTimeFieldOrPropertyInfo( fieldOrPropertyOrIndexer );
    }
}