using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.SyntaxBuilders;
using IExpressionBuilder = Caravela.Framework.Code.SyntaxBuilders.IExpressionBuilder;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.CodeModel.CustomExpressionBuilder
{
    class Aspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            var f = meta.CompileTime( new Fixture("Hello, world.") );
            Console.WriteLine( f );
            var list = meta.CompileTime( new List<Fixture>() { f } );
            var runTimeList = meta.RunTime( list );
            
            return meta.Proceed();
        }
    
    }
    
    class Fixture : IExpressionBuilder
    {
        string _msg;
        
        public Fixture( string msg )
        {
            this._msg = msg;
        }
    
        public IExpression ToExpression()
        {
            ExpressionBuilder builder = new();
            builder.AppendVerbatim( "new " );
            builder.AppendTypeName( typeof(Fixture) );
            builder.AppendVerbatim( "(" );
            builder.AppendLiteral( this._msg );
            builder.AppendVerbatim( ")" );
            return builder.ToExpression();
        }
    }

    class TargetCode
    {
        // <target>
        [Aspect]
        int Method(int a)
        {
            return a;
        }
    }
}