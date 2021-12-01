int Method(int a)
{
    /* 
for ( int i = 0; i < n; i++ )
{
    if ( i == 5 )
    {
        return default(global::Caravela.Framework.Tests.Integration.Templating.Pragma.StatementBuilderT.TargetCode);
    }
    Console.WriteLine("Hello, world.");
}
 */

    for (int i = 0; i < n; i++)
    {
        if (i == 5)
        {
            return default(global::Caravela.Framework.Tests.Integration.Templating.Pragma.StatementBuilderT.TargetCode);
        }

        Console.WriteLine("Hello, world.");
    }

    return default;
}
