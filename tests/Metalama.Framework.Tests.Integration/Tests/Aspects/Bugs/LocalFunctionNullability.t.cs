// Warning CS8321 on `InvokeAsync`: `The local function 'InvokeAsync' is declared but never used`

// Warning CS8321 on `InvokeAsync`: `The local function 'InvokeAsync' is declared but never used`

// Warning CS8619 on `(global::System.Threading.Tasks.Task<global::System.Object>)global::System.Threading.Tasks.Task.FromResult<object?>(null)`: `Nullability of reference types in value of type 'Task<object>' doesn't match target type 'Task<object?>'.`

// Warning CS8619 on `(global::System.Threading.Tasks.Task<global::System.Object>)global::System.Threading.Tasks.Task.FromResult<object?>(null)`: `Nullability of reference types in value of type 'Task<object?>' doesn't match target type 'Task<object>'.`

internal class C
{
    [TheAspect]
    public void M()
    {
        global::System.Threading.Tasks.Task<global::System.Object?> InvokeAsync()
        {
            return (global::System.Threading.Tasks.Task<global::System.Object>)global::System.Threading.Tasks.Task.FromResult<object?>(null);
        }

        return;
    }
}
