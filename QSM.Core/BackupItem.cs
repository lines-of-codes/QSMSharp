namespace QSM.Core
{
    public class BackupItem
    {
        public ushort Id;
        public string Name;
        public DateTime SavedDate;
        /// <summary>
        /// The Uri to the backup
        /// </summary>
        public Uri Uri;
        public bool IsSavedOnline;

        public BackupItem(ushort id, string name, Uri uri, bool isSavedOnline = false)
        {
            Name = name;
            SavedDate = DateTime.UtcNow;
            Uri = uri;
            IsSavedOnline = isSavedOnline;
        }
    }
}
