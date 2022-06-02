internal class Target
    {
        [return: NotNull]
        private string M()
        {
    global::System.String returnValue;
            returnValue = "";
goto __aspect_return_1;
__aspect_return_1:    if (returnValue == null)
    {
        throw new global::System.ArgumentNullException("<return>");
    }

    return returnValue;
        }
    }