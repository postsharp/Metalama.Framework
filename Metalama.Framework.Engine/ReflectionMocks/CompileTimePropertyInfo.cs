// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using System;
using System.Globalization;
using System.Reflection;

namespace Metalama.Framework.Engine.ReflectionMocks
{
    internal class CompileTimePropertyInfo : PropertyInfo, ICompileTimeReflectionObject<IPropertyOrIndexer>
    {
        public ISdkRef<IPropertyOrIndexer> Target { get; set; }

        private CompileTimePropertyInfo( IPropertyOrIndexer property )
        {
            this.Target = property.ToTypedRef();
        }

        public static PropertyInfo Create( IPropertyOrIndexer property ) => new CompileTimePropertyInfo( property );

        public override object[] GetCustomAttributes( bool inherit ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override object[] GetCustomAttributes( Type attributeType, bool inherit ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override bool IsDefined( Type attributeType, bool inherit ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type DeclaringType => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override string Name => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type ReflectedType => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override MethodInfo[] GetAccessors( bool nonPublic ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override MethodInfo GetGetMethod( bool nonPublic ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override ParameterInfo[] GetIndexParameters() => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override MethodInfo GetSetMethod( bool nonPublic ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override object GetValue( object? obj, BindingFlags invokeAttr, Binder? binder, object?[]? index, CultureInfo? culture )
            => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override void SetValue( object? obj, object? value, BindingFlags invokeAttr, Binder? binder, object?[]? index, CultureInfo? culture )
            => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override PropertyAttributes Attributes => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override bool CanRead => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override bool CanWrite => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type PropertyType => throw CompileTimeMocksHelper.CreateNotSupportedException();
    }
}