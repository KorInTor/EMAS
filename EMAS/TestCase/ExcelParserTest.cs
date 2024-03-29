using System.Diagnostics;
using EMAS.Model;
using EMAS.Service.Serialization;
using System.IO;

namespace EMAS.TestCases
{
    //[TestClass] - Задел на будущие тесты используя MSTest.
    public class ExcelParserTest
    {
        private static Equipment exampleEquipment = new("Списан", "REG123", "Описание", "1", "°C", "0…10", 1, "Марка", "Производитель", "Тип", "FN123", ["Tag1", "Tag2"]);

        private static string inputRelativePath = @"..\..\..\Examples\InputExcelExample.xlsx";

        private static string inputFullPath = Path.GetFullPath(inputRelativePath);

        private static string outputRelativePath = @"..\..\..\Examples\OutputExcelExample.xlsx";

        private static string outputFullPath = Path.GetFullPath(outputRelativePath);

        //[TestMethod]
        public void EquipmentInputTest()
        {
            Trace.Listeners.Add(new TextWriterTraceListener("Excel_Result.txt"));
            Trace.AutoFlush = true;

            //var expected = exampleEquipment;
            
            //var actual = ExcelParser.ImportFromExcel(inputFullPath)[0];

            //Assert.AreEqual(expected, actual);

            List<Equipment> equipments = ExcelParser.ImportFromExcel(inputFullPath);

            Trace.WriteLine($"Результат считывания из Excel файла: {equipments[0].Equals(exampleEquipment)}");
        }

        //[TestMethod]
        public void EquipmentOutputTest()
        {
            Trace.Listeners.Add(new TextWriterTraceListener("Excel_Result.txt"));
            Trace.AutoFlush = true;

            //var expected = exampleEquipment;

            //var actual = ExcelParser.ImportFromExcel(outputFullPath)[0];

            //Assert.AreEqual(expected, actual);

            ExcelParser.ExportToExcel(new List <Equipment>{ exampleEquipment },outputFullPath,"Локация-1");

            Trace.WriteLine($"Результат экспорта в Excel файл: {exampleEquipment.Equals(ExcelParser.ImportFromExcel(outputFullPath)[0])}");
        }

        //[TestMethod]
        public void EquipmentInputOutputTest()
        {
            EquipmentInputTest();
            EquipmentOutputTest();
        }
    }
}
