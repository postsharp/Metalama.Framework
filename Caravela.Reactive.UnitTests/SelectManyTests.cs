using Caravela.Reactive;
using Caravela.Reactive.Sources;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

        /*
        [Fact]
        public void ImmutableNestedSelectManyTest()
        {
            var roslynCompilation = CSharpCompilation.Create( null! );

            new[] { new Compilation( roslynCompilation ) }.ToImmutableReactive()
                .SelectMany( c => c.DeclaredTypes.SelectMany( t => t.Attributes ) )
                .GroupBy( a => a.Type )
                .GetValue();
        }


        [Fact]
        public void ReactiveNestedSelectManyTest()
        {
            var roslynCompilation = CSharpCompilation.Create( null! );

            new ReactiveHashSet<Compilation> { new Compilation( roslynCompilation ) }
                .SelectMany( c => c.DeclaredTypes.SelectMany( t => t.Attributes ) )
                .GroupBy( a => a.Type )
                .GetValue();
        }
        */

    }

}
