// This file is automatically generated when you do `Build.ps1 prepare`.

import jetbrains.buildServer.configs.kotlin.v2019_2.*

// Both Swabra and swabra need to be imported
import jetbrains.buildServer.configs.kotlin.v2019_2.buildFeatures.sshAgent
import jetbrains.buildServer.configs.kotlin.v2019_2.buildFeatures.Swabra
import jetbrains.buildServer.configs.kotlin.v2019_2.buildFeatures.swabra
import jetbrains.buildServer.configs.kotlin.v2019_2.failureConditions.*
import jetbrains.buildServer.configs.kotlin.v2019_2.buildSteps.powerShell
import jetbrains.buildServer.configs.kotlin.v2019_2.triggers.*

version = "2021.2"

project {

   buildType(DebugBuild)
   buildType(ReleaseBuild)
   buildType(PublicBuild)
   buildType(PublicDeployment)
   buildType(VersionBump)
   buildTypesOrder = arrayListOf(DebugBuild,ReleaseBuild,PublicBuild,PublicDeployment,VersionBump)
}

object DebugBuild : BuildType({

    name = "Build [Debug]"

    artifactRules = "+:artifacts/publish/public/**/*=>artifacts/publish/public\n+:artifacts/publish/private/**/*=>artifacts/publish/private\n+:artifacts/testResults/**/*=>artifacts/testResults\n-:%system.teamcity.build.tempDir%/Metalama/CrashReports/**/*.dmp=>logs\n+:%system.teamcity.build.tempDir%/Metalama/CrashReports/**/*=>logs\n+:%system.teamcity.build.tempDir%/Metalama/Extract/**/.completed=>logs\n+:%system.teamcity.build.tempDir%/Metalama/ExtractExceptions/**/*=>logs"

    params {
        text("BuildArguments", "", label = "Build Arguments", description = "Arguments to append to the 'Build' build step.", allowEmpty = true)
        text("TimeOut", "300", label = "Time-Out Threshold", description = "Seconds after the duration of the last successful build.", regex = """\d+""", validationMessage = "The timeout has to be an integer number.")
    }
    vcs {
        root(DslContext.settingsRoot)
        root(AbsoluteId("Metalama_Metalama20242_MetalamaFrameworkPrivate"), "+:. => source-dependencies/Metalama.Framework.Private")
    }

    steps {
        powerShell {
            name = "Kill background processes before cleanup"
            scriptMode = file {
                path = "Build.ps1"
            }
            noProfile = false
            param("jetbrains_powershell_scriptArguments", "tools kill")
        }
        powerShell {
            name = "Build"
            scriptMode = file {
                path = "Build.ps1"
            }
            noProfile = false
            param("jetbrains_powershell_scriptArguments", "docker test --configuration Debug --buildNumber %build.number% --buildType %system.teamcity.buildType.id% %BuildArguments%")
        }
    }

    failureConditions {
        failOnMetricChange {
            metric = BuildFailureOnMetric.MetricType.BUILD_DURATION
            units = BuildFailureOnMetric.MetricUnit.DEFAULT_UNIT
            comparison = BuildFailureOnMetric.MetricComparison.MORE
            compareTo = build {
                buildRule = lastSuccessful()
            }
            stopBuildOnFailure = true
            param("metricThreshold", "%TimeOut%")
        }
    }

    requirements {
        equals ("env.BuildAgentType", "DockerWindowsStandard")
    }

    features {
        swabra {
            lockingProcesses = Swabra.LockingProcessPolicy.KILL
            verbose = true
        }
    }

    triggers {
        vcs {
            watchChangesInDependencies = true
            branchFilter = "+:<default>"
            // Build will not trigger automatically if the commit message contains comment value.
            triggerRules = "-:comment=<<VERSION_BUMP>>|<<DEPENDENCIES_UPDATED>>:**"
        }
    }

    dependencies {
        dependency(AbsoluteId("Metalama_Metalama20242_MetalamaBackstage_DebugBuild")) {
            snapshot {
                     onDependencyFailure = FailureAction.FAIL_TO_START
            }

            artifacts {
                cleanDestination = true
                artifactRules = "+:artifacts/publish/private/**/*=>dependencies/Metalama.Backstage"
            }
        }
        dependency(AbsoluteId("Metalama_Metalama20242_MetalamaCompiler_ReleaseBuild")) {
            snapshot {
                     onDependencyFailure = FailureAction.FAIL_TO_START
            }

            artifacts {
                cleanDestination = true
                artifactRules = "+:artifacts/packages/Release/Shipping/**/*=>dependencies/Metalama.Compiler"
            }
        }
        dependency(AbsoluteId("Metalama_Metalama20242_MetalamaFrameworkRunTime_DebugBuild")) {
            snapshot {
                     onDependencyFailure = FailureAction.FAIL_TO_START
            }

            artifacts {
                cleanDestination = true
                artifactRules = "+:artifacts/publish/private/**/*=>dependencies/Metalama.Framework.RunTime"
            }
        }

     }

})

object ReleaseBuild : BuildType({

    name = "Build [Release]"

    artifactRules = "+:artifacts/publish/public/**/*=>artifacts/publish/public\n+:artifacts/publish/private/**/*=>artifacts/publish/private\n+:artifacts/testResults/**/*=>artifacts/testResults\n"

    params {
        text("BuildArguments", "", label = "Build Arguments", description = "Arguments to append to the 'Build' build step.", allowEmpty = true)
        text("TimeOut", "300", label = "Time-Out Threshold", description = "Seconds after the duration of the last successful build.", regex = """\d+""", validationMessage = "The timeout has to be an integer number.")
    }
    vcs {
        root(DslContext.settingsRoot)
        root(AbsoluteId("Metalama_Metalama20242_MetalamaFrameworkPrivate"), "+:. => source-dependencies/Metalama.Framework.Private")
    }

    steps {
        powerShell {
            name = "Kill background processes before cleanup"
            scriptMode = file {
                path = "Build.ps1"
            }
            noProfile = false
            param("jetbrains_powershell_scriptArguments", "tools kill")
        }
        powerShell {
            name = "Build"
            scriptMode = file {
                path = "Build.ps1"
            }
            noProfile = false
            param("jetbrains_powershell_scriptArguments", "docker test --configuration Release --buildNumber %build.number% --buildType %system.teamcity.buildType.id% %BuildArguments%")
        }
    }

    failureConditions {
        failOnMetricChange {
            metric = BuildFailureOnMetric.MetricType.BUILD_DURATION
            units = BuildFailureOnMetric.MetricUnit.DEFAULT_UNIT
            comparison = BuildFailureOnMetric.MetricComparison.MORE
            compareTo = build {
                buildRule = lastSuccessful()
            }
            stopBuildOnFailure = true
            param("metricThreshold", "%TimeOut%")
        }
    }

    requirements {
        equals ("env.BuildAgentType", "DockerWindowsStandard")
    }

    features {
        swabra {
            lockingProcesses = Swabra.LockingProcessPolicy.KILL
            verbose = true
        }
    }

    dependencies {
        dependency(AbsoluteId("Metalama_Metalama20242_MetalamaBackstage_ReleaseBuild")) {
            snapshot {
                     onDependencyFailure = FailureAction.FAIL_TO_START
            }

            artifacts {
                cleanDestination = true
                artifactRules = "+:artifacts/publish/private/**/*=>dependencies/Metalama.Backstage"
            }
        }
        dependency(AbsoluteId("Metalama_Metalama20242_MetalamaCompiler_ReleaseBuild")) {
            snapshot {
                     onDependencyFailure = FailureAction.FAIL_TO_START
            }

            artifacts {
                cleanDestination = true
                artifactRules = "+:artifacts/packages/Release/Shipping/**/*=>dependencies/Metalama.Compiler"
            }
        }
        dependency(AbsoluteId("Metalama_Metalama20242_MetalamaFrameworkRunTime_ReleaseBuild")) {
            snapshot {
                     onDependencyFailure = FailureAction.FAIL_TO_START
            }

            artifacts {
                cleanDestination = true
                artifactRules = "+:artifacts/publish/private/**/*=>dependencies/Metalama.Framework.RunTime"
            }
        }

     }

})

object PublicBuild : BuildType({

    name = "Build [Public]"

    artifactRules = "+:artifacts/publish/public/**/*=>artifacts/publish/public\n+:artifacts/publish/private/**/*=>artifacts/publish/private\n+:artifacts/testResults/**/*=>artifacts/testResults\n"

    params {
        text("UpstreamCheckArguments", "", label = "Check pending upstream changes Arguments", description = "Arguments to append to the 'Check pending upstream changes' build step.", allowEmpty = true)
        text("BuildArguments", "", label = "Build Arguments", description = "Arguments to append to the 'Build' build step.", allowEmpty = true)
        text("TimeOut", "300", label = "Time-Out Threshold", description = "Seconds after the duration of the last successful build.", regex = """\d+""", validationMessage = "The timeout has to be an integer number.")
    }
    vcs {
        root(DslContext.settingsRoot)
        root(AbsoluteId("Metalama_Metalama20242_MetalamaFrameworkPrivate"), "+:. => source-dependencies/Metalama.Framework.Private")
    }

    steps {
        powerShell {
            name = "Kill background processes before cleanup"
            scriptMode = file {
                path = "Build.ps1"
            }
            noProfile = false
            param("jetbrains_powershell_scriptArguments", "tools kill")
        }
        powerShell {
            name = "Check pending upstream changes"
            scriptMode = file {
                path = "Build.ps1"
            }
            noProfile = false
            param("jetbrains_powershell_scriptArguments", "tools git check-upstream %UpstreamCheckArguments%")
        }
        powerShell {
            name = "Build"
            scriptMode = file {
                path = "Build.ps1"
            }
            noProfile = false
            param("jetbrains_powershell_scriptArguments", "docker test --configuration Public --buildNumber %build.number% --buildType %system.teamcity.buildType.id% %BuildArguments%")
        }
    }

    failureConditions {
        failOnMetricChange {
            metric = BuildFailureOnMetric.MetricType.BUILD_DURATION
            units = BuildFailureOnMetric.MetricUnit.DEFAULT_UNIT
            comparison = BuildFailureOnMetric.MetricComparison.MORE
            compareTo = build {
                buildRule = lastSuccessful()
            }
            stopBuildOnFailure = true
            param("metricThreshold", "%TimeOut%")
        }
    }

    requirements {
        equals ("env.BuildAgentType", "DockerWindowsStandard")
    }

    features {
        swabra {
            lockingProcesses = Swabra.LockingProcessPolicy.KILL
            verbose = true
        }
        sshAgent {
            // By convention, the SSH key name is always PostSharp.Engineering for all repositories using SSH to connect.
            teamcitySshKey = "PostSharp.Engineering"
        }
    }

    dependencies {
        dependency(AbsoluteId("Metalama_Metalama20242_MetalamaBackstage_PublicBuild")) {
            snapshot {
                     onDependencyFailure = FailureAction.FAIL_TO_START
            }

            artifacts {
                cleanDestination = true
                artifactRules = "+:artifacts/publish/private/**/*=>dependencies/Metalama.Backstage"
            }
        }
        dependency(AbsoluteId("Metalama_Metalama20242_MetalamaCompiler_PublicBuild")) {
            snapshot {
                     onDependencyFailure = FailureAction.FAIL_TO_START
            }

            artifacts {
                cleanDestination = true
                artifactRules = "+:artifacts/packages/Release/Shipping/**/*=>dependencies/Metalama.Compiler"
            }
        }
        dependency(AbsoluteId("Metalama_Metalama20242_MetalamaFrameworkRunTime_PublicBuild")) {
            snapshot {
                     onDependencyFailure = FailureAction.FAIL_TO_START
            }

            artifacts {
                cleanDestination = true
                artifactRules = "+:artifacts/publish/private/**/*=>dependencies/Metalama.Framework.RunTime"
            }
        }

     }

})

object PublicDeployment : BuildType({

    name = "Deploy [Public]"

    type = Type.DEPLOYMENT

    params {
        text("PublishArguments", "", label = "Publish Arguments", description = "Arguments to append to the 'Publish' build step.", allowEmpty = true)
        text("TimeOut", "300", label = "Time-Out Threshold", description = "Seconds after the duration of the last successful build.", regex = """\d+""", validationMessage = "The timeout has to be an integer number.")
    }
    vcs {
        root(DslContext.settingsRoot)
    }

    steps {
        powerShell {
            name = "Publish"
            scriptMode = file {
                path = "Build.ps1"
            }
            noProfile = false
            param("jetbrains_powershell_scriptArguments", "publish --configuration Public %PublishArguments%")
        }
    }

    failureConditions {
        failOnMetricChange {
            metric = BuildFailureOnMetric.MetricType.BUILD_DURATION
            units = BuildFailureOnMetric.MetricUnit.DEFAULT_UNIT
            comparison = BuildFailureOnMetric.MetricComparison.MORE
            compareTo = build {
                buildRule = lastSuccessful()
            }
            stopBuildOnFailure = true
            param("metricThreshold", "%TimeOut%")
        }
    }

    requirements {
        equals ("env.BuildAgentType", "DockerWindowsStandard")
    }

    features {
        swabra {
            lockingProcesses = Swabra.LockingProcessPolicy.KILL
            verbose = true
        }
        sshAgent {
            // By convention, the SSH key name is always PostSharp.Engineering for all repositories using SSH to connect.
            teamcitySshKey = "PostSharp.Engineering"
        }
    }

    dependencies {
        dependency(AbsoluteId("Metalama_Metalama20242_MetalamaBackstage_PublicBuild")) {
            snapshot {
                     onDependencyFailure = FailureAction.FAIL_TO_START
            }

            artifacts {
                cleanDestination = true
                artifactRules = "+:artifacts/publish/private/**/*=>dependencies/Metalama.Backstage"
            }
        }
        dependency(AbsoluteId("Metalama_Metalama20242_MetalamaBackstage_PublicDeployment")) {
            snapshot {
                     onDependencyFailure = FailureAction.FAIL_TO_START
            }
        }
        dependency(AbsoluteId("Metalama_Metalama20242_MetalamaCompiler_PublicBuild")) {
            snapshot {
                     onDependencyFailure = FailureAction.FAIL_TO_START
            }

            artifacts {
                cleanDestination = true
                artifactRules = "+:artifacts/packages/Release/Shipping/**/*=>dependencies/Metalama.Compiler"
            }
        }
        dependency(AbsoluteId("Metalama_Metalama20242_MetalamaCompiler_PublicDeployment")) {
            snapshot {
                     onDependencyFailure = FailureAction.FAIL_TO_START
            }
        }
        dependency(AbsoluteId("Metalama_Metalama20242_MetalamaFrameworkRunTime_PublicBuild")) {
            snapshot {
                     onDependencyFailure = FailureAction.FAIL_TO_START
            }

            artifacts {
                cleanDestination = true
                artifactRules = "+:artifacts/publish/private/**/*=>dependencies/Metalama.Framework.RunTime"
            }
        }
        dependency(AbsoluteId("Metalama_Metalama20242_MetalamaFrameworkRunTime_PublicDeployment")) {
            snapshot {
                     onDependencyFailure = FailureAction.FAIL_TO_START
            }
        }
        dependency(PublicBuild) {
            snapshot {
                     onDependencyFailure = FailureAction.FAIL_TO_START
            }

            artifacts {
                cleanDestination = true
                artifactRules = "+:artifacts/publish/public/**/*=>artifacts/publish/public\n+:artifacts/publish/private/**/*=>artifacts/publish/private\n+:artifacts/testResults/**/*=>artifacts/testResults"
            }
        }

     }

})

object VersionBump : BuildType({

    name = "Version Bump"

    params {
        text("BumpArguments", "", label = "Bump Arguments", description = "Arguments to append to the 'Bump' build step.", allowEmpty = true)
        text("TimeOut", "300", label = "Time-Out Threshold", description = "Seconds after the duration of the last successful build.", regex = """\d+""", validationMessage = "The timeout has to be an integer number.")
    }
    vcs {
        root(DslContext.settingsRoot)
    }

    steps {
        powerShell {
            name = "Bump"
            scriptMode = file {
                path = "Build.ps1"
            }
            noProfile = false
            param("jetbrains_powershell_scriptArguments", "bump %BumpArguments%")
        }
    }

    failureConditions {
        failOnMetricChange {
            metric = BuildFailureOnMetric.MetricType.BUILD_DURATION
            units = BuildFailureOnMetric.MetricUnit.DEFAULT_UNIT
            comparison = BuildFailureOnMetric.MetricComparison.MORE
            compareTo = build {
                buildRule = lastSuccessful()
            }
            stopBuildOnFailure = true
            param("metricThreshold", "%TimeOut%")
        }
    }

    requirements {
        equals ("env.BuildAgentType", "DockerWindowsStandard")
    }

    features {
        swabra {
            lockingProcesses = Swabra.LockingProcessPolicy.KILL
            verbose = true
        }
        sshAgent {
            // By convention, the SSH key name is always PostSharp.Engineering for all repositories using SSH to connect.
            teamcitySshKey = "PostSharp.Engineering"
        }
    }

})

