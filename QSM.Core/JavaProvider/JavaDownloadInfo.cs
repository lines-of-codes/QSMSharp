using QSM.Core.Utilities;

namespace QSM.Core.JavaProvider;

public sealed record JavaDownloadInfo(string Url, string Hash, HashAlgorithm HashAlgorithm);
