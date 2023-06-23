[ReturnNumbers]
private object Method()
{
    var numbers = new object[]
    {
        42
    };
    if (numbers is { } numbers_1)
        if (global::System.Linq.Enumerable.ToList(numbers_1) is { } list)
            global::System.Linq.Enumerable.ToList(list);
    if (global::System.Linq.Enumerable.ToList(numbers!) is { } list_1)
        global::System.Linq.Enumerable.ToList(list_1);
    if (numbers is { } numbers_2)
        global::System.Linq.Enumerable.ToList(global::System.Linq.Enumerable.ToList(numbers_2));
    return null;
}
