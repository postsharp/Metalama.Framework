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
            MarkedTextSpanSet textSpanSet = new MarkedTextSpanSet();
            
            Assert.Equal( "{ [0..inf)=>Default } ", textSpanSet.ToString() );

        }

        
        [Fact]
        public void OneSpan()
        {
            MarkedTextSpanSet textSpanSet = new MarkedTextSpanSet();
            textSpanSet.Mark( new TextSpan( 0, 10 ), TextSpanCategory.Keyword );
            
            Assert.Equal( "{ [0..10)=>Keyword, [10..inf)=>Default } ", textSpanSet.ToString() );

        }
        
        [Fact]
        public void TwoSpans_Disjoint()
        {
            MarkedTextSpanSet textSpanSet = new MarkedTextSpanSet();
            textSpanSet.Mark( new TextSpan( 0, 10 ), TextSpanCategory.Keyword );
            textSpanSet.Mark( new TextSpan( 15, 10 ), TextSpanCategory.Keyword );
            
            Assert.Equal( "{ [0..10)=>Keyword, [10..15)=>Default, [15..25)=>Keyword, [25..inf)=>Default } ", textSpanSet.ToString() );
        }
        
        [Fact]
        public void TwoSpans_SecondOverlapping()
        {
            MarkedTextSpanSet textSpanSet = new MarkedTextSpanSet();
            textSpanSet.Mark( new TextSpan( 0, 10 ), TextSpanCategory.Variable );
            textSpanSet.Mark( new TextSpan( 5, 10 ), TextSpanCategory.Keyword );
            
            Assert.Equal( "{ [0..5)=>Variable, [5..10)=>Keyword, [10..15)=>Keyword, [15..inf)=>Default } ", textSpanSet.ToString() );
        }
        
        [Fact]
        public void TwoSpans_SecondSuperset()
        {
            MarkedTextSpanSet textSpanSet = new MarkedTextSpanSet();
            textSpanSet.Mark( new TextSpan( 5, 10 ), TextSpanCategory.Variable );
            textSpanSet.Mark( new TextSpan( 0, 20 ), TextSpanCategory.Keyword );
            
            Assert.Equal( "{ [0..5)=>Keyword, [5..15)=>Keyword, [15..20)=>Keyword, [20..inf)=>Default } ", textSpanSet.ToString() );
        }
        
        [Fact]
        public void TwoSpans_Subset_Inner()
        {
            MarkedTextSpanSet textSpanSet = new MarkedTextSpanSet();
            textSpanSet.Mark( new TextSpan( 10, 10 ), TextSpanCategory.Variable );
            textSpanSet.Mark( new TextSpan( 12, 3 ), TextSpanCategory.Keyword );
            
            Assert.Equal( "{ [0..10)=>Default, [10..12)=>Variable, [12..15)=>Keyword, [15..20)=>Variable, [20..inf)=>Default } ", textSpanSet.ToString() );
        }
        
        [Fact]
        public void TwoSpans_Subset_LeftAligned()
        {
            MarkedTextSpanSet textSpanSet = new MarkedTextSpanSet();
            textSpanSet.Mark( new TextSpan( 10, 10 ), TextSpanCategory.Variable );
            textSpanSet.Mark( new TextSpan( 10, 3 ), TextSpanCategory.Keyword );
            
            Assert.Equal( "{ [0..10)=>Default, [10..13)=>Keyword, [13..20)=>Variable, [20..inf)=>Default } ", textSpanSet.ToString() );
        }
        
          
        [Fact]
        public void TwoSpans_Subset_RightAligned()
        {
            MarkedTextSpanSet textSpanSet = new MarkedTextSpanSet();
            textSpanSet.Mark( new TextSpan( 10, 10 ), TextSpanCategory.Variable );
            textSpanSet.Mark( new TextSpan( 17, 3 ), TextSpanCategory.Keyword );
            
            Assert.Equal( "{ [0..10)=>Default, [10..17)=>Variable, [17..20)=>Keyword, [20..inf)=>Default } ", textSpanSet.ToString() );
        }
    }
}