using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests
{
    public class MarkedTextSpanTests
    {
        
        [Fact]
        public void ZeroSpan()
        {
            TextSpanClassifier c = new TextSpanClassifier();
            
            Assert.Equal( "{ [0..inf)=>Default } ", c.ToString() );

        }

        
        [Fact]
        public void OneSpan()
        {
            TextSpanClassifier c = new TextSpanClassifier();
            c.Mark( new TextSpan( 0, 10 ), TextSpanCategory.TemplateKeyword );
            
            Assert.Equal( "{ [0..10)=>Keyword, [10..inf)=>Default } ", c.ToString() );

        }
        
        [Fact]
        public void TwoSpans_Disjoint()
        {
            TextSpanClassifier c = new TextSpanClassifier();
            c.Mark( new TextSpan( 0, 10 ), TextSpanCategory.TemplateKeyword );
            c.Mark( new TextSpan( 15, 10 ), TextSpanCategory.TemplateKeyword );
            
            Assert.Equal( "{ [0..10)=>Keyword, [10..15)=>Default, [15..25)=>Keyword, [25..inf)=>Default } ", c.ToString() );
        }
        
        [Fact]
        public void TwoSpans_SecondOverlapping()
        {
            TextSpanClassifier c = new TextSpanClassifier();
            c.Mark( new TextSpan( 0, 10 ), TextSpanCategory.TemplateVariable );
            c.Mark( new TextSpan( 5, 10 ), TextSpanCategory.TemplateKeyword );
            
            Assert.Equal( "{ [0..5)=>Variable, [5..10)=>Keyword, [10..15)=>Keyword, [15..inf)=>Default } ", c.ToString() );
        }
        
        [Fact]
        public void TwoSpans_SecondSuperset()
        {
            TextSpanClassifier c = new TextSpanClassifier();
            c.Mark( new TextSpan( 5, 10 ), TextSpanCategory.TemplateVariable );
            c.Mark( new TextSpan( 0, 20 ), TextSpanCategory.TemplateKeyword );
            
            Assert.Equal( "{ [0..5)=>Keyword, [5..15)=>Keyword, [15..20)=>Keyword, [20..inf)=>Default } ", c.ToString() );
        }
        
        [Fact]
        public void TwoSpans_Subset_Inner()
        {
            TextSpanClassifier c = new TextSpanClassifier();
            c.Mark( new TextSpan( 10, 10 ), TextSpanCategory.TemplateVariable );
            c.Mark( new TextSpan( 12, 3 ), TextSpanCategory.TemplateKeyword );
            
            Assert.Equal( "{ [0..10)=>Default, [10..12)=>Variable, [12..15)=>Keyword, [15..20)=>Variable, [20..inf)=>Default } ", c.ToString() );
        }
        
        [Fact]
        public void TwoSpans_Subset_LeftAligned()
        {
            TextSpanClassifier c = new TextSpanClassifier();
            c.Mark( new TextSpan( 10, 10 ), TextSpanCategory.TemplateVariable );
            c.Mark( new TextSpan( 10, 3 ), TextSpanCategory.TemplateKeyword );
            
            Assert.Equal( "{ [0..10)=>Default, [10..13)=>Keyword, [13..20)=>Variable, [20..inf)=>Default } ", c.ToString() );
        }
        
          
        [Fact]
        public void TwoSpans_Subset_RightAligned()
        {
            TextSpanClassifier c = new TextSpanClassifier();
            c.Mark( new TextSpan( 10, 10 ), TextSpanCategory.TemplateVariable );
            c.Mark( new TextSpan( 17, 3 ), TextSpanCategory.TemplateKeyword );
            
            Assert.Equal( "{ [0..10)=>Default, [10..17)=>Variable, [17..20)=>Keyword, [20..inf)=>Default } ", c.ToString() );
        }

        [Fact]
        public void TwoSpans_Subset_Weaker()
        {
            TextSpanClassifier c = new TextSpanClassifier();
            c.Mark( new TextSpan( 10, 10 ), TextSpanCategory.TemplateVariable );
            c.Mark( new TextSpan( 12, 3 ), TextSpanCategory.Default );

            Assert.Equal( "{ [0..10)=>Default, [10..20)=>Variable, [20..inf)=>Default } ", c.ToString() );
        }
    }
}