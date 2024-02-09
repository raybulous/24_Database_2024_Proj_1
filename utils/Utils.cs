using System.Text;
using System.Text.Json;
using static _24_Database_2024_Proj_1.Constants;

namespace _24_Database_2024_Proj_1.utils;

public class Utils
{
    // Function to calculate the size of a record in megabytes
    public static double CalculateRecordSize(Record record, Unit unit = Unit.Bytes)
    {
        double sizeInBytes = record.Data.Length;

        return unit switch
        {
            Unit.Bytes => sizeInBytes,
            Unit.Megabytes => sizeInBytes / (1024.0 * 1024.0),
            _ => throw new ArgumentException("Invalid unit specified", nameof(unit))
        };
    }

    // This method should be used to calculate Max records per block.
    public static int MaxRecordCalculator(double SizeOfEachRecord, int MaxBlockSizeBytes = BlockConstants.MaxBlockSizeBytes)
    {
        // Calculate the available space for records after reserving the specified percentage
        double availableSpaceForRecords = MaxBlockSizeBytes;

        // Calculate the maximum number of records based on the available space
        int maxRecords = (int)(availableSpaceForRecords / SizeOfEachRecord);

        return maxRecords;
    }
    
    // This method should be used to calculate Max RESERVE records per block.
    public static double MaxReserveSizeCalculator(int MaxBlockSizeBytes = BlockConstants.MaxBlockSizeBytes, double ReservedSpace = BlockConstants.ReservedSpace)
    {
        // Calculate the available space for records after reserving the specified percentage
        return MaxBlockSizeBytes * (1 - ReservedSpace);
        
    }
    
    // This method should be used to calculate Max RESERVE records per block.
    public static int MaxReserveRecordCalculator(double SizeOfEachRecord, int MaxBlockSizeBytes = BlockConstants.MaxBlockSizeBytes, double ReservedSpace = BlockConstants.ReservedSpace)
    {

        // Calculate the maximum number of records based on the available space
        int maxRecords = (int)(MaxReserveSizeCalculator() / SizeOfEachRecord);

        return maxRecords;
    }

    // This method should be used to calculate Max RESERVE records per block.
    public static int MaxReserveRecordCalculator(Record record, int MaxBlockSizeBytes = BlockConstants.MaxBlockSizeBytes, double ReservedSpace = BlockConstants.ReservedSpace)
    {  
        // Calculate record size
        double sizeOfRecord = CalculateRecordSize(record);

        // Calculate the maximum number of records based on the available space and the size of the record
        int maxRecords = (int)(MaxReserveSizeCalculator() / sizeOfRecord);

        return maxRecords;
    }

    /// <summary>
    /// Converts a value from bytes to megabytes.
    /// </summary>
    /// <param name="givenbyte">The size in bytes to be converted.</param>
    /// <returns>The size in megabytes.</returns>
    public static double ByteToMegaByte(double givenbyte)
    {
        return givenbyte / 1024 / 1024;
    }
    
    //TODO: Create a function to calculate which block will have what range of tconst data

}

// Enum for units of measurement
public enum Unit
{
    Bytes,
    Megabytes
}