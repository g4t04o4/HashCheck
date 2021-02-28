using System;
using System.IO;
using System.Linq;

namespace HashCheck
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                var textFilePath = args[0];
                var directoryPath = args[1];

                var filesToValidateDirectory = new DirectoryInfo(directoryPath);

                var fileSpecs = File.ReadAllLines(textFilePath)
                    .Select(
                        file =>
                        {
                            var split = file.Split(' ');
                            return new FileHashInfo(split[0], HashValidator.ParseHashAlgorithm(split[1]), split[2]);
                        }
                    );

                foreach (var spec in fileSpecs)
                {
                    var result = HashValidator.Validate(spec, filesToValidateDirectory);
                    var resultString = result switch
                    {
                        HashValidationResult.HashCorrect => "OK",
                        HashValidationResult.HashMismatch => "FAIL",
                        HashValidationResult.FileNotFound => "NOT FOUND",
                        _ => throw new ArgumentOutOfRangeException(nameof(result))
                    };

                    Console.WriteLine($"{spec.FileName}: {resultString}");
                }

                return 0;
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine(
                    "Wrong input. You should provide a path to the file with the hash data and a path to the directory. Please, try again.");

                return 1;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("One of the paths you provided were incorrect. Please, try again.");

                return 1;
            }
            // catch (InvalidDataException)
            // {
            //     Console.WriteLine("Your hash data file is incorrect/badly formatted. Please, try again.");
            // }
            catch (Exception)
            {
                Console.WriteLine("Something went horribly wrong.");

                return 1;
            }
        }
    }
}