using System;
using System.IO;
using System.Security.Cryptography;

namespace HashCheck
{
    public static class HashValidator
    {
        public static HashAlgorithm ParseHashAlgorithm(string str)
        {
            return str switch
            {
                "sha1" => HashAlgorithm.Sha1,
                "sha256" => HashAlgorithm.Sha256,
                "md5" => HashAlgorithm.Md5,
                _ => throw new ArgumentOutOfRangeException(nameof(str), str, null)
            };
        }

        public static HashValidationResult Validate(FileHashInfo info, DirectoryInfo filesToValidateDirectory)
        {
            var name = info.FileName;

            var file = new FileInfo(Path.Combine(filesToValidateDirectory.FullName, name));

            if (!file.Exists)
            {
                return HashValidationResult.FileNotFound;
            }

            var bytes = File.ReadAllBytes(file.FullName);
            var hash = GetHash(bytes, info.HashAlgorithm);

            return info.ExpectedHashValue == hash
                ? HashValidationResult.HashCorrect
                : HashValidationResult.HashMismatch;
        }

        private static string GetHash(byte[] inputBytes, HashAlgorithm hashAlgorithm)
        {
            var hashValue = hashAlgorithm switch
            {
                HashAlgorithm.Sha1 => SHA1.Create().ComputeHash(inputBytes),
                HashAlgorithm.Sha256 => SHA256.Create().ComputeHash(inputBytes),
                HashAlgorithm.Md5 => MD5.Create().ComputeHash(inputBytes),
                _ => throw new ArgumentOutOfRangeException(nameof(hashAlgorithm), hashAlgorithm, null)
            };

            var hashString = BitConverter.ToString(hashValue).Replace("-", "").ToLower();
            return hashString;
        }
    }

    public enum HashAlgorithm
    {
        Sha1,
        Sha256,
        Md5
    }

    public enum HashValidationResult
    {
        HashCorrect,
        HashMismatch,
        FileNotFound
    }

    public struct FileHashInfo
    {
        public FileHashInfo(string fileName, HashAlgorithm hashAlgorithm, string expectedHashValue)
        {
            FileName = fileName;
            HashAlgorithm = hashAlgorithm;
            ExpectedHashValue = expectedHashValue;
        }

        public string FileName { get; }
        public HashAlgorithm HashAlgorithm { get; }
        public string ExpectedHashValue { get; }
    }
}