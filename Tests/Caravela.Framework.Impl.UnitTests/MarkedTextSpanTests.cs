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
            ClassifiedTextSpanCollection c = new ClassifiedTextSpanCollection();
            
            Assert.Equal( "{ [0..inf)=>Default } ", c.ToString() );

        }

        
        [Fact]
        public void OneSpan()
        {
            ClassifiedTextSpanCollection c = new ClassifiedTextSpanCollection();
            c.Add( new TextSpan( 0, 10 ), TextSpanClassification.TemplateKeyword );
            
            Assert.Equal( "{ [0..10)=>Keyword, [10..inf)=>Default } ", c.ToString() );

        }
        
        [Fact]
        public void TwoSpans_Disjoint()
        {
            ClassifiedTextSpanCollection c = new ClassifiedTextSpanCollection();
            c.Add( new TextSpan( 0, 10 ), TextSpanClassification.TemplateKeyword );
            c.Add( new TextSpan( 15, 10 ), TextSpanClassification.TemplateKeyword );
            
            Assert.Equal( "{ [0..10)=>Keyword, [10..15)=>Default, [15..25)=>Keyword, [25..inf)=>Default } ", c.ToString() );
        }
        
        [Fact]
        public void TwoSpans_SecondOverlapping()
        {
            ClassifiedTextSpanCollection c = new ClassifiedTextSpanCollection();
            c.Add( new TextSpan( 0, 10 ), TextSpanClassification.CompileTimeVariable );
            c.Add( new TextSpan( 5, 10 ), TextSpanClassification.TemplateKeyword );
            
            Assert.Equal( "{ [0..5)=>Variable, [5..10)=>Keyword, [10..15)=>Keyword, [15..inf)=>Default } ", c.ToString() );
        }
        
        [Fact]
        public void TwoSpans_SecondSuperset()
        {
            ClassifiedTextSpanCollection c = new ClassifiedTextSpanCollection();
            c.Add( new TextSpan( 5, 10 ), TextSpanClassification.CompileTimeVariable );
            c.Add( new TextSpan( 0, 20 ), TextSpanClassification.TemplateKeyword );
            
            Assert.Equal( "{ [0..5)=>Keyword, [5..15)=>Keyword, [15..20)=>Keyword, [20..inf)=>Default } ", c.ToString() );
        }
        
        [Fact]
        public void TwoSpans_Subset_Inner()
        {
            ClassifiedTextSpanCollection c = new ClassifiedTextSpanCollection();
            c.Add( new TextSpan( 10, 10 ), TextSpanClassification.CompileTimeVariable );
            c.Add( new TextSpan( 12, 3 ), TextSpanClassification.TemplateKeyword );
            
            Assert.Equal( "{ [0..10)=>Default, [10..12)=>Variable, [12..15)=>Keyword, [15..20)=>Variable, [20..inf)=>Default } ", c.ToString() );
        }
        
        [Fact]
        public void TwoSpans_Subset_LeftAligned()
        {
            ClassifiedTextSpanCollection c = new ClassifiedTextSpanCollection();
            c.Add( new TextSpan( 10, 10 ), TextSpanClassification.CompileTimeVariable );
            c.Add( new TextSpan( 10, 3 ), TextSpanClassification.TemplateKeyword );
            
            Assert.Equal( "{ [0..10)=>Default, [10..13)=>Keyword, [13..20)=>Variable, [20..inf)=>Default } ", c.ToString() );
        }
        
          
        [Fact]
        public void TwoSpans_Subset_RightAligned()
        {
            ClassifiedTextSpanCollection c = new ClassifiedTextSpanCollection();
            c.Add( new TextSpan( 10, 10 ), TextSpanClassification.CompileTimeVariable );
            c.Add( new TextSpan( 17, 3 ), TextSpanClassification.TemplateKeyword );
            
            Assert.Equal( "{ [0..10)=>Default, [10..17)=>Variable, [17..20)=>Keyword, [20..inf)=>Default } ", c.ToString() );
        }

        [Fact]
        public void TwoSpans_Subset_Weaker()
        {
            ClassifiedTextSpanCollection c = new ClassifiedTextSpanCollection();
            c.Add( new TextSpan( 10, 10 ), TextSpanClassification.CompileTimeVariable );
            c.Add( new TextSpan( 12, 3 ), TextSpanClassification.Default );

            Assert.Equal( "{ [0..10)=>Default, [10..20)=>Variable, [20..inf)=>Default } ", c.ToString() );
        }
    }
}