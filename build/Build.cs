using Colorful;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    const string Ether = "Ether";
    const string Api = "Ether.Api";
    const string EmailGenerator = "Ether.EmailGenerator";

    public static int Main() => Execute<Build>(x => x.Publish);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Path to artifacts directory - Default is /artifacts")]
    readonly AbsolutePath ArtifactsDirectory = RootDirectory / "artifacts";

    [Solution] readonly Solution Solution;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Solution.GetProject("Ether.Tests"))
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .EnableNoRestore());
        });

    Target Publish => _ => _
        .DependsOn(PublishClient)
        .DependsOn(PublishApi)
        .DependsOn(PublishEmailGenerator)
        .Executes();

    Target PublishClient => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            DotNetPublish(s => s
                .SetProject(Solution.GetProject(Ether))
                .SetConfiguration(Configuration)
                .SetOutput(ArtifactsDirectory / Ether));
        });

    Target PublishApi => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            DotNetPublish(s => s
                .SetProject(Solution.GetProject(Api))
                .SetConfiguration(Configuration)
                .SetOutput(ArtifactsDirectory / Api));
        });

    Target PublishEmailGenerator => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            DotNetPublish(s => s
                .SetProject(Solution.GetProject(EmailGenerator))
                .SetConfiguration(Configuration)
                .SetOutput(ArtifactsDirectory / EmailGenerator));
        });

    Target Configure => _ => _
        .Executes(() =>
        {
            DotNet($"user-secrets init --project \"{Solution.GetProject("Ether.Api")}\"");

            var configureDb = ConsoleUtility.PromptForChoice("Do you want to configure database connection string?", (false, "No"), (true, "Yes"));
            if (configureDb)
            {
                var host = ConsoleUtility.PromptForInput("What's the DB host name?", "localhost");
                var port = ConsoleUtility.PromptForInput("What's the DB port?", "27017");
                var username = ConsoleUtility.PromptForInput("Username to use when connecting", "");
                Console.WriteLine("Password:");
                var password = ConsoleUtility.ReadSecret();
                SetSecret("DBConfig:ConnectionString", $"mongodb://{username}:{password}@{host}:{port}");
            }

            var configureAD = ConsoleUtility.PromptForChoice("Do you want to configure AD?", (false, "No"), (true, "Yes"));
            if (configureAD)
            {
                var adType = ConsoleUtility.PromptForChoice("Select AD type",
                    ("Machine", "The computer store. This represents the SAM store."),
                    ("Domain", "The domain store. This represents the AD DS store."),
                    ("ApplicationDirectory", "The application directory store. This represents the AD LDS store."));
                SetSecret("ADConfig:Type", adType);
            }
        });

    private void SetSecret(string name, string value)
    {
        DotNet($"user-secrets set \"{name}\" \"{value}\" --project \"{Solution.GetProject("Ether.Api")}\"");
    }
}
