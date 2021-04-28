// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.TestApp
{
    public class SuppressWarningAttribute : Attribute, IAspect<ICodeElement>
    {
        private readonly string[] codes;

        public SuppressWarningAttribute( params string[] codes )
        {
            this.codes = codes;
        }

        public void Initialize( IAspectBuilder<ICodeElement> aspectBuilder )
        {
            foreach ( var code in this.codes )
            {
                aspectBuilder.SuppressDiagnostic( code, aspectBuilder.TargetDeclaration );
            }
        }
    }
}
