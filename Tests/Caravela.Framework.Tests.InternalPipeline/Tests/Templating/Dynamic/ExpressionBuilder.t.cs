private int Method( int a, string c, DateTime dt )
{
    global::System.Console.WriteLine($"type=object");
    var value = global::Caravela.Framework.Caravela.Framework.Code.ExpressionBuilders.ExpressionBuilderExtensions.ToValue(Test(1, 1d, 1F f, "s\"\n", new global::System.Object[]{1, 2, 3}, a, typeof(global::System.Int32)));
    return default;
}