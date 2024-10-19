using System;

internal class Target
{
    private readonly string _q = default !;

    [NotNull]
    public string Q
    {
        get
        {
            return _q;
        }
        private init
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }

            _q = value;
        }
    }
}