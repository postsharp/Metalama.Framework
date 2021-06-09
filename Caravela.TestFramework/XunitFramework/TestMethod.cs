// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Caravela.TestFramework.XunitFramework
{
    internal class TestMethod : ITestMethod, IMethodInfo
    {
        private readonly TestFactory _factory;
        private readonly string _relativePath;

        public TestMethod( TestFactory factory, string relativePath )
        {
            this._factory = factory;
            this._relativePath = relativePath;
        }

        void IXunitSerializable.Deserialize( IXunitSerializationInfo info ) => throw new NotImplementedException();

        void IXunitSerializable.Serialize( IXunitSerializationInfo info ) => throw new NotImplementedException();

        IMethodInfo ITestMethod.Method => this;

        ITestClass ITestMethod.TestClass => this._factory.GetTestType( Path.GetDirectoryName( this._relativePath )! );

        IEnumerable<IAttributeInfo> IMethodInfo.GetCustomAttributes( string assemblyQualifiedAttributeTypeName ) => Enumerable.Empty<IAttributeInfo>();

        IEnumerable<ITypeInfo> IMethodInfo.GetGenericArguments() => Enumerable.Empty<ITypeInfo>();

        IEnumerable<IParameterInfo> IMethodInfo.GetParameters() => Enumerable.Empty<IParameterInfo>();

        IMethodInfo IMethodInfo.MakeGenericMethod( params ITypeInfo[] typeArguments ) => throw new NotSupportedException();

        bool IMethodInfo.IsAbstract => false;

        bool IMethodInfo.IsGenericMethodDefinition => false;

        bool IMethodInfo.IsPublic => true;

        bool IMethodInfo.IsStatic => true;

        string IMethodInfo.Name => Path.GetFileNameWithoutExtension( this._relativePath );

        ITypeInfo IMethodInfo.ReturnType => new ReflectionTypeInfo( typeof(void) );

        ITypeInfo IMethodInfo.Type => this._factory.GetTestType( Path.GetDirectoryName( this._relativePath )! );
    }
}