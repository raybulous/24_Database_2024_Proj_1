using System.Text;
using System.Text.Json;

namespace _24_Database_2024_Proj_1.utils;

public class RecordSizeCalculator
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

}

// Enum for units of measurement
public enum Unit
{
    Bytes,
    Megabytes
}