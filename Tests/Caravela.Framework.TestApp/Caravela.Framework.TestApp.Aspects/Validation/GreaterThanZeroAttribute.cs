﻿using System;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.TestApp.Aspects.Validation
{
    [CompileTime]
    public class GreaterThanZeroAttribute : ValidateAttribute
    {
        public override void Validate(string name, dynamic value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(name, "The value must be strictly greater than zero.");
            }
        }
    }
}
