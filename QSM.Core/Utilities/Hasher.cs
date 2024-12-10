using System.Security.Cryptography;

namespace QSM.Core.Utilities
{
	public class Hasher
	{
		public static string GetFileHash(HashAlgorithm algorithm, string path)
		{
			return algorithm switch
			{
				HashAlgorithm.SHA256 => SHA256.Create().GetFileHashAsString(path),
				HashAlgorithm.SHA512 => SHA512.Create().GetFileHashAsString(path),
				_ => throw new InvalidOperationException("Unsupported hash algorithm used in parameter.")
			};
		}
	}
}
