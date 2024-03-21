using ClosedXML.Excel;
using EMAS.Model;

namespace EMAS.Service.Serialization
{
    public static class ExcelParser
    {
        public static List<Equipment> ImportFromExcel(string filePath)
        {
            var equipments = new List<Equipment>();

            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RangeUsed().RowsUsed();

                foreach (var row in rows)
                {
                    if (row.RowNumber() == 1) // Пропускаем заголовок
                        continue;

                    var tags = row.Cell(12).GetValue<string>().Split(','); // Тэги (Через запятую!)
                    var equipment = new Equipment(
                        id: row.Cell(1).GetValue<int>(), // Уникальный идентификатор
                        type: row.Cell(2).GetValue<string>(), // Тип
                        name: row.Cell(3).GetValue<string>(), // Марка
                        limit: row.Cell(4).GetValue<string>(), // Предел
                        units: row.Cell(5).GetValue<string>(), // Единицы измерения
                        accuracyClass: row.Cell(6).GetValue<string>(), // Класс точности
                        status: row.Cell(7).GetValue<string>(), // Статус
                        description: row.Cell(8).GetValue<string>(), // Описание
                        manufacturer: row.Cell(9).GetValue<string>(), // Производитель
                        registrationNumber: row.Cell(10).GetValue<string>(), // Регистрационный номер
                        factoryNumber: row.Cell(11).GetValue<string>(), // Серийный (Заводской) Номер
                        tags: new List<string>(tags)
                    );

                    equipments.Add(equipment);
                }
            }

            return equipments;
        }

        public static void ExportToExcel(List<Equipment> equipments, string filePath, string locationName)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add($"{locationName}");

            // Заголовки столбцов
            worksheet.Cell(1, 1).Value = "Уникальный идентификатор";
            worksheet.Cell(1, 2).Value = "Тип";
            worksheet.Cell(1, 3).Value = "Марка";
            worksheet.Cell(1, 4).Value = "Предел";
            worksheet.Cell(1, 5).Value = "Единицы измерения";
            worksheet.Cell(1, 6).Value = "Класс точности";
            worksheet.Cell(1, 7).Value = "Статус";
            worksheet.Cell(1, 8).Value = "Описание";
            worksheet.Cell(1, 9).Value = "Производитель";
            worksheet.Cell(1, 10).Value = "Регистрационный номер";
            worksheet.Cell(1, 11).Value = "Серийный (Заводской) Номер"; 
            worksheet.Cell(1, 12).Value = "Тэги (Через запятую!)";

            int rowNumber = 2;

            foreach (var equipment in equipments)
            {
                worksheet.Cell(rowNumber, 1).Value = equipment.Id;
                worksheet.Cell(rowNumber, 2).Value = equipment.Type;
                worksheet.Cell(rowNumber, 3).Value = equipment.Name;
                worksheet.Cell(rowNumber, 4).Value = equipment.Limit;
                worksheet.Cell(rowNumber, 5).Value = equipment.Units;
                worksheet.Cell(rowNumber, 6).Value = equipment.AccuracyClass;
                worksheet.Cell(rowNumber, 7).Value = equipment.Status;
                worksheet.Cell(rowNumber, 8).Value = equipment.Description;
                worksheet.Cell(rowNumber, 9).Value = equipment.Manufacturer;
                worksheet.Cell(rowNumber, 10).Value = equipment.RegistrationNumber;
                worksheet.Cell(rowNumber, 11).Value = equipment.FactoryNumber;
                worksheet.Cell(rowNumber, 12).Value = string.Join(",", equipment.Tags);

                rowNumber++;
            }

            workbook.SaveAs(filePath);
        }
		
		public static void ExportToExcel(Dictionary<string,List<Equipment>> equipmentListsOnLocatinos, string filePath)
		{
			foreach (string locationName in equipmentListsOnLocatinos.Keys)
			{
                ExportToExcel(equipmentListsOnLocatinos[locationName], filePath, locationName);
			}
		}
		
        public static void CreateBlankExcel(string filePath)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Наименование местоположения");

            // Заголовки столбцов
            worksheet.Cell(1, 1).Value = "Уникальный идентификатор";
            worksheet.Cell(1, 2).Value = "Тип";
            worksheet.Cell(1, 3).Value = "Марка";
            worksheet.Cell(1, 4).Value = "Предел";
            worksheet.Cell(1, 5).Value = "Единицы измерения";
            worksheet.Cell(1, 6).Value = "Класс точности";
            worksheet.Cell(1, 7).Value = "Статус";
            worksheet.Cell(1, 8).Value = "Описание";
            worksheet.Cell(1, 9).Value = "Производитель";
            worksheet.Cell(1, 10).Value = "Регистрационный номер";
            worksheet.Cell(1, 11).Value = "Серийный (Заводской) Номер";
            worksheet.Cell(1, 12).Value = "Тэги (Через запятую!)";

            workbook.SaveAs(filePath);
        }
    }
}
