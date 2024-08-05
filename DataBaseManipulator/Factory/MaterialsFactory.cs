using Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseManipulator.Factory
{
    internal class MaterialsFactory
    {
        private static Random random = new();

        private static string[] Names =
            {
            "SteelMaster Supplies", "IronGuard Essentials", "ForgePro Consumables", "MetalWorks Maintenance Kit", "SteelCare Solutions", "IronShield Supplies", "ForgeGuard Essentials",
              "MetalMaster Maintenance Pack", "SteelPro Consumables", "IronWorks Essentials", "ForgeCare Supplies", "SteelShield Solutions", "MetalGuard Maintenance Kit", "IronMaster Consumables",
              "ForgeWorks Essentials", "MetalCare Maintenance Pack", "SteelGuard Supplies", "IronPro Essentials", "ForgeMaster Maintenance Kit"
            };
        private static string[] Types =
         {
            "Моторное масло", "Трансмиссионное масло", "Гидравлическое масло", "Масло для редукторов",
            "Масло для компрессоров", "Масло для цепей", "Сферические подшипники", "Упорные подшипники",
            "Игольчатые подшипники", "Упорные подшипники", "Цепи для редукторов", "Синхронные ремни",
            "Плоские ремни", "Крыльчатки", "Уплотнительные кольца", "Средства для удаления ржавчины",
            "Охлаждающая жидкость на основе полиалкиленгликоля (PAG)", "Охлаждающая жидкость на основе полиальфаолефина (PAO)", "Стрелки датчиков", "Сверло спиральное",
            "Сверло ступеньчатое", "Воздушные фильтры", "Водные растворы гликолей", "Масляные фильтры",
            "Металлические прокладки", "Электроды для плазменной сварки", "Электроды для аргонодуговой сварки", "Электроды для плазменной сварки",
            "Твердосплавные резцы", "Круги для резки металла", "Обезжириватели", "Фторопластовые уплотнители"
        };
        private static string[] Units =
            {"Ящики", "Коробки", "Бочки", "Канистры" };
        private static string[] StorageTypes =
            {"Склад","Вагон","Вне помещения"};
        private static string[] DescriptionSamples =
            {string.Empty, string.Empty, string.Empty, string.Empty};
        private static string[] Comments =
            {string.Empty, string.Empty, string.Empty, string.Empty};
        

        public static int GenerateAmount(int amountToGenerate = 200)
        {

            return random.Next(1,amountToGenerate);
        }

        public static string GenerateUnits()
        {
            return Units[random.Next(0, Units.Length - 1)];
        }

        public static string GenerateTypes()
        {
            return Types[random.Next(0, Types.Length - 1)];
        }

        public static string GenerateStorageTypes()
        {
            return StorageTypes[random.Next(0, StorageTypes.Length - 1)];
        }

        public static string GenerateInventoryNumber(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GenerateName()
        {
            return Names[random.Next(Names.Length - 1)];
        }

        public static string GenerateDescription()
        {
            return DescriptionSamples[random.Next(0, DescriptionSamples.Length - 1)];
        }

        public static List<MaterialPiece> GenerateMaterials(int amountToGenerate)
        {
            List<MaterialPiece> materials = new List<MaterialPiece>();
           
            for(int i = 0; i < amountToGenerate; i++)
            {
                MaterialPiece randMaterialPiece = new MaterialPiece();
                randMaterialPiece.Amount = GenerateAmount(amountToGenerate);
                randMaterialPiece.Comment = string.Empty;
                randMaterialPiece.StorageType = GenerateStorageTypes();
                randMaterialPiece.Extras = string.Empty;
                randMaterialPiece.Name = GenerateName();
                randMaterialPiece.InventoryNumber = GenerateInventoryNumber();
                randMaterialPiece.Type = GenerateTypes();
                randMaterialPiece.Description = GenerateDescription();
                randMaterialPiece.Units = GenerateUnits();

                materials.Add(randMaterialPiece);
                Debug.WriteLine(randMaterialPiece.Name);
            }

            return materials;
        }

    }
}
