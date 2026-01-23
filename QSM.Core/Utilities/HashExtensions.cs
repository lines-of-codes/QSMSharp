using System.Security.Cryptography;

namespace QSM.Core.Utilities;

public static class HashExtensions
{
	extension(SHA512 sha512)
	{
		public string GetFileHashAsString(string filePath)
		{
			using FileStream stream = File.OpenRead(filePath);
			return sha512.GetStreamHashAsString(stream);
		}

		public string GetStreamHashAsString(Stream stream)
		{
			byte[] hashBytes = sha512.ComputeHash(stream);
			return Convert.ToHexStringLower(hashBytes);
		}
	}

	extension(SHA256 sha256)
	{
		public string GetFileHashAsString(string filePath)
		{
			using FileStream stream = File.OpenRead(filePath);
			return sha256.GetStreamHashAsString(stream);
		}

		public string GetStreamHashAsString(Stream stream)
		{
			byte[] hashBytes = sha256.ComputeHash(stream);
			return Convert.ToHexStringLower(hashBytes);
		}
	}

	extension(SHA1 sha1)
	{
		public string GetFileHashAsString(string filePath)
		{
			using FileStream stream = File.OpenRead(filePath);
			return sha1.GetStreamHashAsString(stream);
		}

		public string GetStreamHashAsString(Stream stream)
		{
			byte[] hashBytes = sha1.ComputeHash(stream);
			return Convert.ToHexStringLower(hashBytes);
		}
	}
}