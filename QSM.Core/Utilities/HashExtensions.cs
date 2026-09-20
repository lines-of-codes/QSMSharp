using SysHashAlgorithm = System.Security.Cryptography.HashAlgorithm;

namespace QSM.Core.Utilities;

public static class HashExtensions
{
	extension(SysHashAlgorithm alg)
	{
		public string GetFileHashAsString(string filePath)
		{
			using FileStream stream = File.OpenRead(filePath);
			return alg.GetStreamHashAsString(stream);
		}

		public string GetStreamHashAsString(Stream stream)
		{
			byte[] hashBytes = alg.ComputeHash(stream);
			return Convert.ToHexStringLower(hashBytes);
		}
	}
}