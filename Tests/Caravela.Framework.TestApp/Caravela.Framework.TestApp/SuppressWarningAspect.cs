﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using System;

namespace Caravela.Framework.TestApp
{
    public class SuppressWarningAttribute : Attribute, IAspect<ICodeElement>
    {
        static SuppressionDefinition _mySuppression1 = new(  "CS1998" );
        static SuppressionDefinition _mySuppression2 = new(  "IDE0051" );

        public SuppressWarningAttribute(  )
        {
            
        }

        public void Initialize( IAspectBuilder<ICodeElement> aspectBuilder )
        {
                aspectBuilder.Diagnostics.Suppress( _mySuppression1 );
            aspectBuilder.Diagnostics.Suppress( _mySuppression2 );
        }
    }
}
