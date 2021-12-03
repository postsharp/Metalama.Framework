// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime.Serialization;
using Caravela.Framework.Serialization;
using System;
using System.IO;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.CompileTime.Serialization
{

    public class AdvancedClassSerializationTests
    {
        [Fact]
        public void CyclicGraph_Classes()
        {
            var mother = new Parent( "gall anonim" );
            var ch1 = new Child { Mother = mother, Name = "ch1" };
            var ch2 = new Child { Mother = mother, Name = "ch2" };
            var ch3 = new Child { Mother = mother, Name = "ch3" };
            mother.Children = new Child[3];
            mother.Children[0] = ch1;
            mother.Children[1] = ch2;
            mother.Children[2] = ch3;

            var formatter = new MetaFormatter();
            var memoryStream = new MemoryStream();
            formatter.Serialize( mother, memoryStream );
            memoryStream.Seek( 0, SeekOrigin.Begin );
            var deserializedObject = (Parent) formatter.Deserialize( memoryStream );

            Assert.Equal( mother.Name, deserializedObject.Name );
            Assert.Equal( mother.Children.Length, deserializedObject.Children.Length );
            Assert.Same( deserializedObject, deserializedObject.Children[0].Mother );
            Assert.Same( deserializedObject, deserializedObject.Children[1].Mother );
            Assert.Same( deserializedObject, deserializedObject.Children[2].Mother );
        }

        [Fact]
        public void CyclicGraph_Arrays()
        {
            var brother = new Child { Sibling = new Child[1], Name = "James" };
            var sister = new Child { Sibling = new Child[1], Name = "Joan" };
            brother.Sibling[0] = sister;
            sister.Sibling[0] = brother;

            var formatter = new MetaFormatter();
            var memoryStream = new MemoryStream();
            formatter.Serialize( brother, memoryStream );
            memoryStream.Seek( 0, SeekOrigin.Begin );
            var deserializedObject = (Child) formatter.Deserialize( memoryStream );

            Assert.NotNull( deserializedObject );
            Assert.Equal( brother.Name, deserializedObject.Name );
            Assert.Equal( brother.Sibling.Length, deserializedObject.Sibling.Length );
            Assert.Equal( sister.Sibling.Length, deserializedObject.Sibling[0].Sibling.Length );

            Assert.Same( deserializedObject, deserializedObject.Sibling[0].Sibling[0] );
        }

        [Fact]
        public void CyclicGraph_RelatedObjectsInArray_Arrays()
        {
            var children = new Child[2];
            var brother = new Child { Sibling = new Child[1], Name = "James" };
            var sister = new Child { Sibling = new Child[1], Name = "Joan" };
            brother.Sibling[0] = sister;
            sister.Sibling[0] = brother;
            children[0] = brother;
            children[1] = sister;

            var formatter = new MetaFormatter();
            var memoryStream = new MemoryStream();
            formatter.Serialize( children, memoryStream );
            memoryStream.Seek( 0, SeekOrigin.Begin );
            var deserializedObject = (Child[]) formatter.Deserialize( memoryStream );

            Assert.NotNull( deserializedObject );
            Assert.Equal( children.Length, deserializedObject.Length );
            Assert.Equal( children[0].Name, deserializedObject[0].Name );
            Assert.Equal( children[1].Name, deserializedObject[1].Name );
            Assert.Equal( brother.Sibling.Length, deserializedObject[0].Sibling.Length );
            Assert.Equal( sister.Sibling.Length, deserializedObject[1].Sibling.Length );

            Assert.Same( deserializedObject[0], deserializedObject[1].Sibling[0] );
            Assert.Same( deserializedObject[1], deserializedObject[0].Sibling[0] );
        }

        [Fact]
        public void CyclicGraph_ToSelf()
        {
            var spouse1 = new Parent( "Mono" );
            spouse1.Spouse = spouse1;

            var formatter = new MetaFormatter();
            var memoryStream = new MemoryStream();
            formatter.Serialize( spouse1, memoryStream );
            memoryStream.Seek( 0, SeekOrigin.Begin );
            var deserializedObject = (Parent) formatter.Deserialize( memoryStream );

            Assert.NotNull( deserializedObject );
            Assert.NotNull( deserializedObject.Spouse );
            Assert.Equal( spouse1.Name, deserializedObject.Name );
            Assert.Equal( spouse1.Name, deserializedObject.Spouse.Name );
            Assert.Same( deserializedObject, deserializedObject.Spouse );
        }

        [MetaSerializer( typeof( Serializer ) )]
        public class Parent
        {
            public string Name;

            public Parent Spouse { get; set; }

            public Parent( string name )
            {
                this.Name = name;
            }

            public Child[] Children { get; set; }

            public class Serializer : ReferenceTypeSerializer<Parent>
            {
                private const string childrenKey = "_ch";
                private const string nameKey = "_";
                private const string spouseKey = "_s";

                public override object CreateInstance( Type type, IArgumentsReader constructorArguments )
                {
                    return new Parent( constructorArguments.GetValue<string>( nameKey ) );
                }

                public override void SerializeObject( Parent obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
                {
                    initializationArguments.SetValue( childrenKey, obj.Children );
                    initializationArguments.SetValue( spouseKey, obj.Spouse );
                    constructorArguments.SetValue( nameKey, obj.Name );
                }

                public override void DeserializeFields( Parent obj, IArgumentsReader initializationArguments )
                {
                    obj.Children = initializationArguments.GetValue<Child[]>( childrenKey );
                    obj.Spouse = initializationArguments.GetValue<Parent>( spouseKey );
                }
            }
        }

        [MetaSerializer( typeof( Serializer ) )]
        public class Child
        {
            public string? Name { get; set; }

            public Parent? Mother { get; set; }

            public Parent? Father { get; set; }

            public Child[]? Sibling { get; set; }

            public class Serializer : ReferenceTypeSerializer<Child>
            {
                private const string _nameKey = "_n";
                private const string _motherKey = "_m";
                private const string _fatherKey = "_f";
                private const string _siblingKey = "_s";

                public override object CreateInstance( Type type, IArgumentsReader constructorArguments )
                {
                    return new Child();
                }

                public override void SerializeObject( Child obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
                {
                    initializationArguments.SetValue( _nameKey, obj.Name );
                    initializationArguments.SetValue( _motherKey, obj.Mother );
                    initializationArguments.SetValue( _fatherKey, obj.Father );
                    initializationArguments.SetValue( _siblingKey, obj.Sibling );
                }

                public override void DeserializeFields( Child obj, IArgumentsReader initializationArguments )
                {
                    obj.Name = initializationArguments.GetValue<string>( _nameKey );
                    obj.Mother = initializationArguments.GetValue<Parent>( _motherKey );
                    obj.Father = initializationArguments.GetValue<Parent>( _fatherKey );
                    obj.Sibling = initializationArguments.GetValue<Child[]>( _siblingKey );
                }
            }
        }

        [MetaSerializer( typeof( Serializer ) )]
        public class IgnoringType
        {
#pragma warning disable IDE0051 // Remove unused private members
            private const string _ignoredKey = "_n";
            private const string _importantKey = "_i";

            [MetaNonSerialized] public string? NoMatter;
#pragma warning restore IDE0051 // Remove unused private members

            public int ImportantValue { get; set; }

            public class Serializer : ReferenceTypeSerializer<IgnoringType>
            {
                public override object CreateInstance( Type type, IArgumentsReader constructorArguments )
                {
                    return new IgnoringType();
                }

                public override void SerializeObject( IgnoringType obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
                {
                    throw new NotImplementedException();
                }

                public override void DeserializeFields( IgnoringType obj, IArgumentsReader initializationArguments )
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}