// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using System;

namespace Caravela.Framework.Impl.ReflectionMocks
{
    internal class CompileTimeFieldOrPropertyInfo : FieldOrPropertyInfo
    {
        public IFieldOrProperty FieldOrProperty { get; }

        private CompileTimeFieldOrPropertyInfo( IFieldOrProperty fieldOrProperty )
        {
            this.FieldOrProperty = fieldOrProperty;
        }

        public static FieldOrPropertyInfo Create( IFieldOrProperty fieldOrProperty ) => new CompileTimeFieldOrPropertyInfo( fieldOrProperty );

        public static FieldOrPropertyInfo Create( IFieldOrPropertyInvocation fieldOrProperty )
            => fieldOrProperty switch
            {
                Property property => Create( property ),
                Field field => Create( field ),
                _ => throw new ArgumentOutOfRangeException( nameof(fieldOrProperty) )
            };
    }
}