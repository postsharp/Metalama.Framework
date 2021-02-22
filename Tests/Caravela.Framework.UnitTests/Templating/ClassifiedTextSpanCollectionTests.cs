using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Caravela.Framework.UnitTests
{
    public class ClassifiedTextSpanCollectionTests
    {

        [Fact]
        public void ZeroSpan()
        {
            var c = new ClassifiedTextSpanCollection();

            Xunit.Assert.Equal( "{ [0..inf)=>Default } ", c.ToString() );
        }

        [Fact]
        public void OneSpan()
        {
            var c = new ClassifiedTextSpanCollection
            {
                { new TextSpan( 0, 10 ), TextSpanClassification.TemplateKeyword }
            };

            Xunit.Assert.Equal( "{ [0..10)=>TemplateKeyword, [10..inf)=>Default } ", c.ToString() );
        }

        [Fact]
        public void TwoSpans_Disjoint()
        {
            var c = new ClassifiedTextSpanCollection
            {
                { new TextSpan( 0, 10 ), TextSpanClassification.TemplateKeyword },
                { new TextSpan( 15, 10 ), TextSpanClassification.TemplateKeyword }
            };

            Xunit.Assert.Equal( "{ [0..10)=>TemplateKeyword, [10..15)=>Default, [15..25)=>TemplateKeyword, [25..inf)=>Default } ", c.ToString() );
        }

        [Fact]
        public void TwoSpans_SecondOverlapping()
        {
            var c = new ClassifiedTextSpanCollection
            {
                { new TextSpan( 0, 10 ), TextSpanClassification.CompileTimeVariable },
                { new TextSpan( 5, 10 ), TextSpanClassification.TemplateKeyword }
            };

            Xunit.Assert.Equal( "{ [0..5)=>CompileTimeVariable, [5..10)=>TemplateKeyword, [10..15)=>TemplateKeyword, [15..inf)=>Default } ", c.ToString() );
        }

        [Fact]
        public void TwoSpans_SecondSuperset()
        {
            var c = new ClassifiedTextSpanCollection
            {
                { new TextSpan( 5, 10 ), TextSpanClassification.CompileTimeVariable },
                { new TextSpan( 0, 20 ), TextSpanClassification.TemplateKeyword }
            };

            Xunit.Assert.Equal( "{ [0..5)=>TemplateKeyword, [5..15)=>TemplateKeyword, [15..20)=>TemplateKeyword, [20..inf)=>Default } ", c.ToString() );
        }

        [Fact]
        public void TwoSpans_Subset_Inner()
        {
            var c = new ClassifiedTextSpanCollection
            {
                { new TextSpan( 10, 10 ), TextSpanClassification.CompileTimeVariable },
                { new TextSpan( 12, 3 ), TextSpanClassification.TemplateKeyword }
            };

            Xunit.Assert.Equal( "{ [0..10)=>Default, [10..12)=>CompileTimeVariable, [12..15)=>TemplateKeyword, [15..20)=>CompileTimeVariable, [20..inf)=>Default } ", c.ToString() );
        }

        [Fact]
        public void TwoSpans_Subset_LeftAligned()
        {
            var c = new ClassifiedTextSpanCollection
            {
                { new TextSpan( 10, 10 ), TextSpanClassification.CompileTimeVariable },
                { new TextSpan( 10, 3 ), TextSpanClassification.TemplateKeyword }
            };

            Xunit.Assert.Equal( "{ [0..10)=>Default, [10..13)=>TemplateKeyword, [13..20)=>CompileTimeVariable, [20..inf)=>Default } ", c.ToString() );
        }

        [Fact]
        public void TwoSpans_Subset_RightAligned()
        {
            var c = new ClassifiedTextSpanCollection
            {
                { new TextSpan( 10, 10 ), TextSpanClassification.CompileTimeVariable },
                { new TextSpan( 17, 3 ), TextSpanClassification.TemplateKeyword }
            };

            Xunit.Assert.Equal( "{ [0..10)=>Default, [10..17)=>CompileTimeVariable, [17..20)=>TemplateKeyword, [20..inf)=>Default } ", c.ToString() );
        }

        [Fact]
        public void TwoSpans_Subset_Weaker()
        {
            var c = new ClassifiedTextSpanCollection
            {
                { new TextSpan( 10, 10 ), TextSpanClassification.CompileTimeVariable },
                { new TextSpan( 12, 3 ), TextSpanClassification.Default }
            };

            Xunit.Assert.Equal( "{ [0..10)=>Default, [10..20)=>CompileTimeVariable, [20..inf)=>Default } ", c.ToString() );
        }
    }
}