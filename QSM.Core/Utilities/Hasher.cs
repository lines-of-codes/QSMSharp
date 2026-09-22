using System.Security.Cryptography;

namespace QSM.Core.Utilities;

public static class Hasher
{
	public static string GetFileHash(HashAlgorithm algorithm, string path)
	{
		if (algorithm == HashAlgorithm.None) return string.Empty;
		
		using FileStream stream = File.OpenRead(path);
		
		// ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
		byte[] hashBytes = algorithm switch
		{
			HashAlgorithm.Sha1 => SHA1.HashData(stream), // NOSONAR
			HashAlgorithm.Sha256 => SHA256.HashData(stream),
			HashAlgorithm.Sha512 => SHA512.HashData(stream),
			_ => throw new InvalidOperationException("Unsupported hash algorithm used in parameter.")
		};
		
		return Convert.ToHexStringLower(hashBytes);
	}
}