using System;

namespace dbrestore
{
  public class RestoreHeader
  {
    public string FilePath { get; set; }
    public string BackupName { get; set; }
    public string BackupDescription { get; set; }
    public int BackupType { get; set; }
    public DateTime ExpirationDate { get; set; }
    public bool Compressed { get; set; }
    public int Position { get; set; }
    public int DeviceType { get; set; }
    public string UserName { get; set; }
    public string ServerName { get; set; }
    public string DatabaseName { get; set; }
    public int DatabaseVersion { get; set; }
    public DateTime DatabaseCreationDate { get; set; }
    public decimal BackupSize { get; set; }
    public decimal FirstLSN { get; set; }
    public decimal LastLSN { get; set; }
    public decimal CheckpointLSN { get; set; }
    public decimal DatabaseBackupLSN { get; set; }
    public DateTime BackupStartDate { get; set; }
    public DateTime BackupFinishDate { get; set; }
    public int SortOrder { get; set; }
    public int CodePage { get; set; }
    public int UnicodeLocaleId { get; set; }
    public int UnicodeComparisonStyle { get; set; }
    public int CompatibilityLevel { get; set; }
    public int SoftwareVendorId { get; set; }
    public int SoftwareVersionMajor { get; set; }
    public int SoftwareVersionMinor { get; set; }
    public int SoftwareVersionBuild { get; set; }
    public string MachineName { get; set; }
    public int Flags { get; set; }
    public Guid BindingID { get; set; }
    public Guid RecoveryForkID { get; set; }
    public string Collation { get; set; }
    public Guid FamilyGUID { get; set; }
    public bool HasBulkLoggedData { get; set; }
    public bool IsSnapshot { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsSingleUser { get; set; }
    public bool HasBackupChecksums { get; set; }
    public bool IsDamaged { get; set; }
    public bool BeginsLogChain { get; set; }
    public bool HasIncompleteMetaData { get; set; }
    public bool IsForceOffline { get; set; }
    public bool IsCopyOnly { get; set; }
    public Guid FirstRecoveryForkID { get; set; }
    public decimal ForkPointLSN { get; set; }
    public string RecoveryModel { get; set; }
    public decimal DifferentialBaseLSN { get; set; }
    public Guid DifferentialBaseGUID { get; set; }
    public string BackupTypeDescription { get; set; }
    public Guid BackupSetGUID { get; set; }
    public long CompressedBackupSize { get; set; }
    public int containment { get; set; }
    public string KeyAlgorithm { get; set; }
    public byte[] EncryptorThumbprint { get; set; }
    public string EncryptorType { get; set; }
  }
}