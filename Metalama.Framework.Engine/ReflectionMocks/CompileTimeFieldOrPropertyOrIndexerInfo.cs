// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.RunTime;

namespace Metalama.Framework.Engine.ReflectionMocks
{
    internal class CompileTimeFieldOrPropertyOrIndexerInfo : FieldOrPropertyOrIndexerInfo
    {
        public IFieldOrPropertyOrIndexer FieldOrPropertyIndexer { get; }

        private CompileTimeFieldOrPropertyOrIndexerInfo( IFieldOrPropertyOrIndexer fieldOrPropertyOrIndexer )
        {
            this.FieldOrPropertyIndexer = fieldOrPropertyOrIndexer;
        }

        public static FieldOrPropertyOrIndexerInfo Create( IFieldOrPropertyOrIndexer fieldOrPropertyOrIndexer ) => new CompileTimeFieldOrPropertyOrIndexerInfo( fieldOrPropertyOrIndexer );
    }
}