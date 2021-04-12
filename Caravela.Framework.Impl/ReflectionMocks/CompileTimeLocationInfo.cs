// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Reflection;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;

namespace Caravela.Framework.Impl.ReflectionMocks
{
    internal class CompileTimeLocationInfo : LocationInfo
    {
        public Property? Property { get; }

        public Field? Field { get; }

        public CompileTimeLocationInfo( Property property ) : base( (PropertyInfo) null! )
        {
            this.Property = property;
        }

        public CompileTimeLocationInfo( Field field ) : base( (FieldInfo) null! )
        {
            this.Field = field;
        }

        public static CompileTimeLocationInfo Create( IFieldOrPropertyInvocation fieldOrProperty ) =>
            fieldOrProperty switch
            {
                Property property => new CompileTimeLocationInfo( property ),
                Field field => new CompileTimeLocationInfo( field ),
                _ => throw new ArgumentOutOfRangeException( nameof( fieldOrProperty ) )
            };
    }
}