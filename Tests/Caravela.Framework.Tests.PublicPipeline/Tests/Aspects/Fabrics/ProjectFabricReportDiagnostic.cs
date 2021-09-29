using System;
using System.Linq;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Fabrics;
using Caravela.Framework.Diagnostics;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Fabrics.ProjectFabricReportDiagnostic
{
    class Fabric : IProjectFabric
    {
        static DiagnosticDefinition _warning = new DiagnosticDefinition( "MY01", Severity.Warning, "Warning" );
        
        public void BuildProject( IProjectFabricBuilder builder )
        {
            foreach ( var m in builder.Target.Types.SelectMany( m => m.Methods ) )
            {
                builder.Diagnostics.Report( m, _warning );
            }
        }
    }

    // <target>
    class TargetCode
    {
        int Method1(int a) => a;
        string Method2(string s) => s;
        
    }
}