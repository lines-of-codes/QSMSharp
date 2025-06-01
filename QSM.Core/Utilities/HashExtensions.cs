using System.Security.Cryptography;

namespace QSM.Core.Utilities;

public static class HashExtensions
{
	public static string GetFileHashAsString(this SHA512 sha512, string filePath)
	{
		using FileStream stream = File.OpenRead(filePath);
		return GetStreamHashAsString(sha512, stream);
	}

	public static string GetStreamHashAsString(this SHA512 sha512, Stream stream)
	{
		byte[] hashBytes = sha512.ComputeHash(stream);
		return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
	}

	public static string GetFileHashAsString(this SHA256 sha256, string filePath)
	{
		using FileStream stream = File.OpenRead(filePath);
		return GetStreamHashAsString(sha256, stream);
	}

	public static string GetStreamHashAsString(this SHA256 sha256, Stream stream)
	{
		byte[] hashBytes = sha256.ComputeHash(stream);
		return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
	}
}