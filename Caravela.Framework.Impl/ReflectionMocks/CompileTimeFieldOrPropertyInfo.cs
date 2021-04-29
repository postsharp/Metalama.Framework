// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using System;
using System.Reflection;

namespace Caravela.Framework.Impl.ReflectionMocks
{
    internal class CompileTimeFieldOrPropertyInfo : FieldOrPropertyInfo
    {
        public IFieldOrProperty FieldOrProperty { get; }

        public CompileTimeFieldOrPropertyInfo( IFieldOrProperty fieldOrProperty )
        {
            this.FieldOrProperty = fieldOrProperty;
        }


        public static CompileTimeFieldOrPropertyInfo Create( IFieldOrPropertyInvocation fieldOrProperty )
            => fieldOrProperty switch
            {
                Property property => new CompileTimeFieldOrPropertyInfo( property ),
                Field field => new CompileTimeFieldOrPropertyInfo( field ),
                _ => throw new ArgumentOutOfRangeException( nameof(fieldOrProperty) )
            };
    }
}