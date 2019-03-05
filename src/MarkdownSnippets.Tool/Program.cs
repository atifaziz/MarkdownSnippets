using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using MarkdownSnippets;

class Program
{
    static Task Main(string[] args)
    {
        var command = new RootCommand
        {
            Description = "A .NET Core Global Tool for merging snippets into markdown documents."
        };
        command.AddOption(BuildTargetDirectoryOption());
        command.Handler = CommandHandler.Create<DirectoryInfo>(
            targetDirectory =>
            {
                if (!TryGetTargetDirectoryPath(targetDirectory, out var targetPath))
                {
                    return 1;
                }

                Console.WriteLine($"  --target-directory: {targetPath}");
                return Inner(targetPath);
            });
        return command.InvokeAsync(args);
    }

    static Option BuildTargetDirectoryOption()
    {
        var argument = new Argument<DirectoryInfo>
        {
            Arity = ArgumentArity.ZeroOrOne
        };
        argument.ExistingOnly();
        return new Option(
            alias: "--target-directory",
            description: "The directory to target for processing. Omit to use the current directory. If the current directory contains a git repository, that is a directory tree that contains a directory names .git.")
        {
            Argument = argument
        };
    }

    static bool TryGetTargetDirectoryPath(DirectoryInfo targetDirectory, out string targetDirectoryPath)
    {
        targetDirectoryPath = null;
        if (targetDirectory != null)
        {
            targetDirectoryPath = targetDirectory.FullName;
            return true;
        }

        var currentDirectory = Environment.CurrentDirectory;
        if (GitRepoDirectoryFinder.IsInGitRepository(currentDirectory))
        {
            targetDirectoryPath = currentDirectory;
            return true;
        }

        Console.WriteLine($"The current directory does no exist with a .git repository. Pass in a target directory instead. Current directory: {currentDirectory}");
        return false;
    }

    static int Inner(string targetDirectory)
    {
        GitHubMarkdownProcessor.Log = Console.WriteLine;
        try
        {
            GitHubMarkdownProcessor.Run(targetDirectory);
        }
        catch (SnippetReadingException exception)
        {
            Console.WriteLine($"Failed to read snippets: {exception.Message}");
            return 1;
        }
        catch (MissingSnippetsException exception)
        {
            Console.WriteLine($"Failed to process markdown: {exception.Message}");
            return 1;
        }
        catch (MarkdownProcessingException exception)
        {
            Console.WriteLine($"Failed to process markdown files: {exception.Message}");
            return 1;
        }
        return 0;
    }
}