namespace _24_Database_2024_Proj_1;
using static utils.Utils;

public class Constants
{
    public static class RecordConstants
    {
        public const int TConstLength = 10;
        public const int FloatSize = sizeof(float);
        public const int IntSize = sizeof(int);
        
        private static Record _record = new Record("tt9999999", 5.6f, 164151);
        public static double RecordSize = CalculateRecordSize(_record); // Potential overheads but shldnt be an issue

    }
    public static class BlockConstants
    {
        // Block Constants
        public const int MaxBlockSizeBytes = 200; // 200B in bytes
        
        /// <summary>
        /// This constant should not be used. Refer to [utils.Utils.MaxRecordCalculator] for the recommended approach.
        /// </summary>
        public const int MaxRecordsPerBlock = 100; // **Should not be used**
        public const double ReservedSpace = 0.1; // 10% of Disk to be spared.
    }

    public static class DiskConstants
    {
        // Disk Constants
        public const int MaxDiskSizeBytes = 300 * 1024 * 1024;
    }

}