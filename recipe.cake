#load nuget:?package=Cake.Recipe&version=3.1.1

var standardNotificationMessage = "Version {0} of {1} has just been released, it will be available here https://www.nuget.org/packages/{1}, once package indexing is complete.";

Environment.SetVariableNames();

BuildParameters.SetParameters(
  context: Context,
  buildSystem: BuildSystem,
  sourceDirectoryPath: "./src",
  title: "GitHubMilestoneCleaner",
  masterBranchName: "main",
  repositoryOwner: "nils-org",
  shouldRunDotNetCorePack: true,
  preferredBuildProviderType: BuildProviderType.GitHubActions,
  twitterMessage: standardNotificationMessage,
  shouldRunCodecov: false,
  shouldRunIntegrationTests: false);

BuildParameters.PrintParameters(Context);

ToolSettings.SetToolPreprocessorDirectives(
  gitReleaseManagerGlobalTool: "#tool dotnet:?package=GitReleaseManager.Tool&version=0.17.0");

ToolSettings.SetToolSettings(context: Context);

Build.RunDotNetCore();