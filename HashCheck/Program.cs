using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HashCheck
{
    internal static class Program
    {
        private class AppOptions
        {
            public readonly FileInfo SpecsFile;
            public readonly DirectoryInfo FilesForValidationDirectory;

            public AppOptions(FileInfo specsFile, DirectoryInfo filesForValidationDirectory)
            {
                SpecsFile = specsFile;
                FilesForValidationDirectory = filesForValidationDirectory;
            }
        }

        private class CommandLineArgumentsParseResult
        {
            public bool IsSuccess { get; }
            public AppOptions Options { get; }
            public string FailReason { get; }

            public CommandLineArgumentsParseResult(AppOptions options)
            {
                IsSuccess = true;
                Options = options;
            }

            public CommandLineArgumentsParseResult(string failReason)
            {
                IsSuccess = false;
                FailReason = failReason;
            }
        }

        private static CommandLineArgumentsParseResult TryParseCommandLineArguments(IReadOnlyList<string> args)
        {
            if (args.Count != 2)
            {
                return new CommandLineArgumentsParseResult($"Two command line arguments expected, got {args.Count}.");
            }

            if (string.IsNullOrWhiteSpace(args[0]))
            {
                return new CommandLineArgumentsParseResult("First argument was empty.");
            }

            if (string.IsNullOrWhiteSpace(args[1]))
            {
                return new CommandLineArgumentsParseResult("Second argument was empty.");
            }

            FileInfo specsFile;
            try
            {
                specsFile = new FileInfo(args[0]);
            }
            catch (Exception)
            {
                return new CommandLineArgumentsParseResult("Specs file path was provided in incorrect format.");
            }

            DirectoryInfo filesToValidateDirectory;
            try
            {
                filesToValidateDirectory = new DirectoryInfo(args[1]);
            }
            catch (Exception)
            {
                return new CommandLineArgumentsParseResult("Specs file path was provided in incorrect format.");
            }

            if (!specsFile.Exists)
            {
                return new CommandLineArgumentsParseResult("Specs file does not exist.");
            }

            if (!filesToValidateDirectory.Exists)
            {
                return new CommandLineArgumentsParseResult(
                    "Provided directory with files for validation does not exist.");
            }

            return new CommandLineArgumentsParseResult(new AppOptions(specsFile, filesToValidateDirectory));
        }

        private static void Run(AppOptions options)
        {
            var fileSpecs = File.ReadAllLines(options.SpecsFile.FullName)
                .Select(
                    file =>
                    {
                        var split = file.Split(' ');
                        return new FileHashInfo(split[0], HashValidator.ParseHashAlgorithm(split[1]), split[2]);
                    }
                );

            foreach (var spec in fileSpecs)
            {
                var result = HashValidator.Validate(spec, options.FilesForValidationDirectory);
                var resultString = result switch
                {
                    HashValidationResult.HashCorrect => "OK",
                    HashValidationResult.HashMismatch => "FAIL",
                    HashValidationResult.FileNotFound => "NOT FOUND",
                    _ => throw new ArgumentOutOfRangeException(nameof(result))
                };

                Console.WriteLine($"{spec.FileName}: {resultString}");
            }
        }

        private static int Main(string[] args)
        {
            var parsingResult = TryParseCommandLineArguments(args);
            if (!parsingResult.IsSuccess)
            {
                Console.Error.WriteLine("Error occured while parsing command line arguments: ");
                Console.Error.WriteLine(parsingResult.FailReason);
                return 1;
            }

            var options = parsingResult.Options;
            try
            {
                Run(options);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error occured while running application:");
                Console.Error.WriteLine(ex.Message);
                return 1;
            }

            return 0;
        }
    }
}