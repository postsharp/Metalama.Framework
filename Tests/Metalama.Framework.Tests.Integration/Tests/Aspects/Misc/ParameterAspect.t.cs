class Class
{
    private void M( [Required] object? a, [Required]  object? b )
    {
    if (b == null)
    {
        throw new global::System.ArgumentNullException("b");
    }

        if (a == null)
    {
        throw new global::System.ArgumentNullException("a");
    }

        goto __aspect_return_1;

__aspect_return_1:    return;
        
    }
}
