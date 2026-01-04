// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.Security.Cryptography;
using System.Text;

namespace ReverseCoverage.TargetModel.Utilities;

/// <summary>
/// Provides stable hashing for method identifiers.
/// </summary>
public static class MethodIdHasher
{
    /// <summary>
    /// Creates a stable hash identifier for a method based on assembly, type, and signature.
    /// </summary>
    /// <param name="assemblyName">The assembly name.</param>
    /// <param name="typeFullName">The fully qualified type name.</param>
    /// <param name="methodSignature">The method signature.</param>
    /// <returns>A stable hash string.</returns>
    public static string CreateMethodId(string assemblyName, string typeFullName, string methodSignature)
    {
        var input = $"{assemblyName}|{typeFullName}|{methodSignature}";
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

