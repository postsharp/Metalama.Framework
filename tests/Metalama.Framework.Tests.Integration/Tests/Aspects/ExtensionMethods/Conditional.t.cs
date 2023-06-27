[ReturnNumbers]
private object Method()
{
    var numbers = new object[]
    {
        42
    };
    switch (global::System.DateTime.Today.DayOfWeek)
    {
        case global::System.DayOfWeek.Monday:
            return (global::System.Object)(numbers is { } numbers_1 ? global::System.Linq.Enumerable.ToList(numbers_1) : null);
        case global::System.DayOfWeek.Tuesday:
            return (global::System.Object)(numbers is { } numbers_2 ? global::System.Linq.Enumerable.ToList(global::System.Linq.Enumerable.ToList(numbers_2)) : null);
        case global::System.DayOfWeek.Wednesday:
            return (global::System.Object)(global::System.Linq.Enumerable.ToList(numbers) is { } list ? global::System.Linq.Enumerable.ToList(list) : null);
        default:
            return (global::System.Object)(numbers is { } numbers_3 ? global::System.Linq.Enumerable.ToList(numbers_3) is { } list_1 ? global::System.Linq.Enumerable.ToList(list_1) : null : null);
    }
}
