#:sdk Cake.Sdk
#:package Cake.BuildSystems.Module@8.0.0

Setup(context=> new BuildData(
    "artifacts",
    new DotNetMSBuildSettings
    {
        Version = GitHubActions.IsRunningOnGitHubActions
                    ?  $"{DateTime.UtcNow.Year}.{DateTime.UtcNow.Month}.{DateTime.UtcNow.Day}.{GitHubActions.Environment.Workflow.RunNumber}"
                    : $"{DateTime.UtcNow.Year}.{DateTime.UtcNow.Month}.{DateTime.UtcNow.Day}"
    }.SetConfiguration("Release")
    ));

Task("Restore")
    .Does<BuildData>((context, data)=> DotNetRestore(
        "src",
        new DotNetRestoreSettings
        {
            MSBuildSettings = data.MSBuildSettings
        }
        ));

Task("Build")
    .IsDependentOn("Restore")
    .Does<BuildData>((context, data)=>
    DotNetBuild(
        "src",
        new DotNetBuildSettings
        {
            NoRestore = true,
            MSBuildSettings = data.MSBuildSettings
        }
        )
    );

Task("Test")
    .IsDependentOn("Build")
    .Does<BuildData>((context, data)=>
    DotNetTest(
        "src",
        new DotNetTestSettings
        {
            NoRestore = true,
            NoBuild = true,
            MSBuildSettings = data.MSBuildSettings
        }
        ));


Task("Pack")
    .IsDependentOn("Test")
    .Does<BuildData>((context, data)=>DotNetPack(
        "./src/HelloWorldApp.Common/HelloWorldApp.Common.csproj",
        new 
        DotNetPackSettings
        {
            NoRestore = true,
            NoBuild = true,
            MSBuildSettings = data.MSBuildSettings,
            OutputDirectory = data.Artifacts
        }
        ));

Task("UploadArtifacts")
    .IsDependentOn("Pack")
    .Does(
        ()=>GitHubActions.Commands.UploadArtifact(Directory("artifacts"), "Swenug")       
    );


Task("Default")
    .IsDependentOn("Pack");

Task("GitHubActions")
    .IsDependentOn("UploadArtifacts");

RunTarget(Argument("target", "Default"));


public record BuildData(
    DirectoryPath Artifacts,
    DotNetMSBuildSettings MSBuildSettings
);