using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.SyntaxBuilders.SwitchTuple;

public class TheAspect : TypeAspect
{
    [Introduce]
    public void TheMethod( string a, string b )
    {
        var switchBuilder = new SwitchStatementBuilder( ExpressionFactory.Capture( ( a, b ) ) );

        foreach (var property in meta.Target.Type.Properties)
        {
            var statementBuilder = new StatementBuilder();
            statementBuilder.AppendTypeName( typeof(Console) );
            statementBuilder.AppendVerbatim( ".WriteLine(" );
            statementBuilder.AppendLiteral( property.Name );
            statementBuilder.AppendVerbatim( ");" );
            switchBuilder.AddCase( new SwitchStatementLabel( property.Name, "xxx" ), statementBuilder.ToStatement().AsList() );
        }

        switchBuilder.AddDefault( StatementFactory.Parse( "return;" ).AsList(), false );
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