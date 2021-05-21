// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.References;
using System;
using System.Globalization;
using System.Reflection;

namespace Caravela.Framework.Impl.ReflectionMocks
{
    internal class CompileTimePropertyInfo : PropertyInfo, ICompileTimeReflectionObject<IProperty>
    {
        public IDeclarationRef<IProperty> Target { get; set; }

        private CompileTimePropertyInfo( IProperty property )
        {
            this.Target = property.ToRef();
        }

        public static PropertyInfo Create( IProperty property ) => new CompileTimePropertyInfo( property );

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

        public override object GetValue( object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture )
            => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override void SetValue( object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture )
            => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override PropertyAttributes Attributes => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override bool CanRead => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override bool CanWrite => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type PropertyType => throw CompileTimeMocksHelper.CreateNotSupportedException();
    }
}