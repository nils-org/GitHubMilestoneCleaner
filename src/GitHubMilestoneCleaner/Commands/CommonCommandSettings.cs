using System.ComponentModel;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GitHubMilestoneCleaner.Commands;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class CommonCommandSettings : CommandSettings
{
    [Description("Owner of the repository.")]
    [CommandOption("-o|--owner")]
    public string Owner { get; set; } = string.Empty;

    [Description("Repository name.")]
    [CommandOption("-r|--repository")] 
    public string Repository { get; set; } = string.Empty;

    [Description("Token (PAT) used to access the repository.")]
    [CommandOption("-t|--token")] 
    public string Token { get; set; } = string.Empty;
        
    public static ValidationResult Validate(CommandContext context, CommonCommandSettings settings)
    {
        if (string.IsNullOrEmpty(settings.Owner))
        {
            return ValidationResult.Error("Owner is required.");
        }

        if (string.IsNullOrEmpty(settings.Repository))
        {
            return ValidationResult.Error("Repository is required.");
        }

        if (string.IsNullOrEmpty(settings.Token))
        {
            return ValidationResult.Error("Token is required.");
        }
            
        return ValidationResult.Success();
    }
}