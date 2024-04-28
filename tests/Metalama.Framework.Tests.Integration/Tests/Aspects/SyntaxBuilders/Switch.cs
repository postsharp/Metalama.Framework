using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.SyntaxBuilders.Switch;

public class TheAspect : TypeAspect
{
    [Introduce]
    public void TheMethod( string propertyName )
    {
        var switchBuilder = new SwitchStatementBuilder( ExpressionFactory.Capture( propertyName ) );

        foreach (var property in meta.Target.Type.Properties)
        {
            var statementBuilder = new StatementBuilder();
            statementBuilder.AppendTypeName( typeof(Console) );
            statementBuilder.AppendVerbatim( ".WriteLine(" );
            statementBuilder.AppendLiteral( property.Name );
            statementBuilder.AppendVerbatim( ");" );
            switchBuilder.AddCase( ExpressionFactory.Literal( property.Name ), statementBuilder.ToStatement() );
        }

        switchBuilder.AddDefault( StatementFactory.Parse( "return;" ), false );
        meta.InsertStatement( switchBuilder.ToStatement() );
    }
}

// <target>
[TheAspect]
internal class C
{
    private string A { get; set; }

    private string B { get; set; }
}