using Caravela.Reactive;
using Caravela.Reactive.Sources;
using System.Collections.Immutable;
using Xunit;

namespace Caravela.Reactive.UnitTests
{

    public class SelectManyTests
    {

        [Fact]
        public void SelectManyTest()
        {
            var compilation = new TestCompilation();

            var memberNames = from type in compilation.Types
                              from member in type.Members
                              select $"{type.Name}.{member.Name}";

            var baseTypeNames = from type in compilation.Types
                                from baseType in type.BaseTypes
                                select $"{type.Name} : {baseType}";

            Assert.Empty( memberNames.GetValue( default ) );
            Assert.Empty( baseTypeNames.GetValue( default ) );

            var c = new SourceType( "C", ImmutableList.Create( "B" ) );

            compilation.Types.Add( c );

            Assert.Empty( memberNames.GetValue( default ) );
            Assert.Equal( new[] { "C : B" }, baseTypeNames.GetValue( default ) );

            c.Members.Add( new Member( "M" ) );

            Assert.Equal( new[] { "C.M" }, memberNames.GetValue( default ) );
        }

        [Fact]
        public void ImmutableNestedSelectManyTest()
        {
            var groups = new[] { new TestCompilation() }.ToImmutableReactive()
                .SelectMany( c => c.Types.SelectMany( t => t.Members ) )
                .GroupBy( a => a.Name )
                .GetValue();

            Assert.Empty( groups );
        }

        [Fact]
        public void ReactiveNestedSelectManyTest()
        {
            var groups = new ReactiveHashSet<TestCompilation> { new() }
                .SelectMany( c => c.Types.SelectMany( t => t.Members ) )
                .GroupBy( a => a.Name )
                .GetValue();

            Assert.Empty( groups );
        }


        [Fact]
        public void SelectManyReactiveWithWhereTest()
        {
            var compilation = new TestCompilation();

            compilation.Types.Add( new( "C", null ) );

            var codeElements = compilation.Types.SelectManyRecursive( type => type.NestedTypes.Where( m => true ) );

            Assert.Single( codeElements.GetValue() );
        }


        [Fact]
        public void SelectManyImmutableWithWhereTest()
        {
            var types = new[] { new SourceType( "C", null ) }.ToImmutableReactive();

            var codeElements = types.SelectManyRecursive( type => type.NestedTypes.Where( m => true ) );

            Assert.Single( codeElements.GetValue() );
        }
    }
}
