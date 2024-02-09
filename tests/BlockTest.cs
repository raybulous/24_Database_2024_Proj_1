//TODO: Unit test?

// namespace _24_Database_2024_Proj_1.tests;
//
// public class BlockTest
// {
//     [TestFixture]
//     public class BlockTests
//     {
//         [Test]
//         public void TestAddRecord()
//         {
//             // Setup
//             var block = new Block(Constants.BlockConstants.MaxBlockSizeBytes);
//             var record = new Record("tt0000001", 5.6f, 1645); // Assuming the Record class exists and matches the structure used in the scenario
//             
//             // Capture the initial state
//             var initialAvailableSpace = block.GetAvailableSpace();
//             var initialAvailableSlots = block.GetAvailableSlots();
//             var initialAvailableReservedSlots = block.GetAvailableReservedSlots();
//
//             // Act
//             var result = block.AddRecord(record);
//
//             // Assert
//             Assert.IsTrue(result == 1, "Record should be added successfully.");
//             Assert.AreEqual(initialAvailableSpace - Utils.CalculateRecordSize(record), block.GetAvailableSpace(), "Available space did not decrease by the expected amount.");
//             Assert.AreEqual(initialAvailableSlots - 1, block.GetAvailableSlots(), "Available slots did not decrease by 1.");
//             Assert.AreEqual(initialAvailableReservedSlots - 1, block.GetAvailableReservedSlots(), "Available reserved slots did not decrease by 1.");
//         }
//     }
// }