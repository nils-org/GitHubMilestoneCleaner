using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GitHubBumpVersionCleaner.Commands
{
    public class CommonCommandSettings : CommandSettings
    {
        [Description("Owner of the repository.")]
        [CommandOption("-o|--owner")]
        public string Owner { get; init; }

        [Description("Repository name.")]
        [CommandOption("-r|--repository")] 
        public string Repository { get; init; }

        [Description("Token (PAT) used to access the repository.")]
        [CommandOption("-t|--token")] 
        public string Token { get; init; }

        [Description("Milestone to clean.")]
        [CommandOption("-m|--milestone")] 
        public string Milestone { get; init; }
            
        [Description("Include closed milestones. Default is to search only in open milestones.")]
        [CommandOption("-c|--closed")]
        [DefaultValue(false)]
        public bool SearchClosedMilestones { get; init; }
        
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
}