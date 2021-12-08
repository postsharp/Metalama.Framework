// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Impl.CodeModel;
using Metalama.Framework.Impl.CodeModel.References;
using System;
using System.Globalization;
using System.Reflection;

namespace Metalama.Framework.Impl.ReflectionMocks
{
    internal class CompileTimeFieldInfo : FieldInfo, ICompileTimeReflectionObject<IField>
    {
        public ISdkRef<IField> Target { get; set; }

        private CompileTimeFieldInfo( IField field )
        {
            this.Target = field.ToTypedRef();
        }

        public static FieldInfo Create( IField field ) => new CompileTimeFieldInfo( field );

        public override object[] GetCustomAttributes( bool inherit ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override object[] GetCustomAttributes( Type attributeType, bool inherit ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override bool IsDefined( Type attributeType, bool inherit ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type DeclaringType => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override string Name => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type ReflectedType => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override FieldAttributes Attributes => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override RuntimeFieldHandle FieldHandle => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type FieldType => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override object GetValue( object obj ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override void SetValue( object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture )
            => throw CompileTimeMocksHelper.CreateNotSupportedException();
    }
}