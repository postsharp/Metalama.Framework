// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit.Abstractions;

namespace Caravela.TestFramework.XunitFramework
{
    internal class TestClass : ITypeInfo, ITestClass
    {
        private readonly TestFactory _testFactory;
        private readonly string _relativePath;

        public TestClass( TestFactory factory, string relativePath )
        {
            this._testFactory = factory;
            this._relativePath = relativePath;
        }

        IEnumerable<IAttributeInfo> ITypeInfo.GetCustomAttributes( string assemblyQualifiedAttributeTypeName ) => Enumerable.Empty<IAttributeInfo>();

        IEnumerable<ITypeInfo> ITypeInfo.GetGenericArguments() => Enumerable.Empty<ITypeInfo>();

        IMethodInfo ITypeInfo.GetMethod( string methodName, bool includePrivateMethod ) => throw new NotImplementedException();

        IEnumerable<IMethodInfo> ITypeInfo.GetMethods( bool includePrivateMethods ) => throw new NotImplementedException();

        IAssemblyInfo ITypeInfo.Assembly => this._testFactory.AssemblyInfo;

        ITypeInfo ITypeInfo.BaseType => null!;

        IEnumerable<ITypeInfo> ITypeInfo.Interfaces => Enumerable.Empty<ITypeInfo>();

        bool ITypeInfo.IsAbstract => false;

        bool ITypeInfo.IsGenericParameter => false;

        bool ITypeInfo.IsGenericType => false;

        bool ITypeInfo.IsSealed => false;

        bool ITypeInfo.IsValueType => false;

        string ITypeInfo.Name => this._relativePath.Replace( Path.DirectorySeparatorChar, '.' );

        void IXunitSerializable.Deserialize( IXunitSerializationInfo info ) => throw new NotImplementedException();

        void IXunitSerializable.Serialize( IXunitSerializationInfo info ) => throw new NotImplementedException();

        ITypeInfo ITestClass.Class => this;

        ITestCollection ITestClass.TestCollection => this._testFactory.Collection;
    }
}