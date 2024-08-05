using DocumentFormat.OpenXml.Drawing;
using Model;

namespace DataBaseManipulator.Factory
{
    public static class EquipmentFactory
    {
        private static Random random = new();

        private static string[] Names =
        {
            "PPS",
            "АМТ-",
            "АЦМ-",
            "АЦМ-",
            "САМТ-",
            "САФ.КАМА-",
            "АЦМ-4УР-",
            "АЦМ-6УИ-",
            "ГИС.МУ-",
            "САФ.МУ-",
            "УМТ-",
            "LLT-MS-",
            "Rosemount ",
            "VEGAFLEX ",
            "DL-",
            "Vibra ALE ",
            "ВЛТЭ-",
            "Гиря 2 кг F",
            "Rosemount ",
            "ДРГ.М-",
            "ЭМИС-ВИХРЬ ",
            "Micro Motion ",
            "OPTIMASS ",
            "ROTAMASS",
            "Штрай-Масс ",
            "ЭЛМЕТРО-Фломак",
            "RABO",
            "RVG ",
            "Rosemount ",
            "Rosemount ",
            "SITRANS P DSIII ",
            "АИР-",
            "АИР-",
            "ДМ",
            "ИД-F-И ",
            "Метран-",
            "ЭнИ-",
            "ДМ ",
            "ДНМ-",
            "Газконтроль-",
            "ДГС ЭРИС-",
            "ОГС-ПГП/М ",
            "СГОЭС ",
            "АНКАТ-",
            "Микросенс ",
            "МУЛЬТИГАЗСЕНС-",
            "ГСМ-",
            "СГГ-",
            "ТС",
            "Fluke ",
            "Кельвин ",
            "Кельвин Компакт ",
            "Rosemount ",
            "SITRANS ",
            "Метран-",
            "ТПУ ",
            "ТПУ-",
            "ТС-Б-У ",
            "ТСПУ ",
            "Rosemount ",
            "КОДОС ",
            "УВП-",
            "МИКОНТ-"
        };


        private static string[] Types = {
            "Автономный манометр-термометр глубинный",
            "Автономный манометр-термометр устьевой",
            "Уровнемер магнитострикционный",
            "Уровнемер микроволновый",
            "Весы",
            "Гиря класса точности F2",
            "Расходомер вихревой",
            "Расходомер массовый",
            "Счетчик газа",
            "Датчик избыточного давления",
            "Датчик перепада давления",
            "Манометр электроконтактный",
            "Напоромер",
            "Газоанализатор инфракрасный оптический",
            "Газоанализатор портативный",
            "Газоанализатор термокаталитический",
            "Датчик температуры",
            "Пирометр",
            "Термопреобразователь с унифицированным выходным сигналом",
            "Термопреобразователь сопротивления",
            "Комплексы медицинского осмотра",
            "Вычислитель расхода",
            "Контроллер",
        };

        private static string[] Manufacturers =
        {
            "Фирма \"Pioneer Petrotech Services Inc.\", Канада",
            "ООО \"Грант-Софт\", г.Уфа",
            "ООО \"ПКФ \"Геотех\", Республика Башкортостан, г. Нефтекамск",
            "ООО ТНПВО \"СИАМ\", г.Томск",
            "ООО Производственное предприятие-лаборатория \"САФ\", г.Набережные Челны",
            "ООО \"КАМАГИС\", г. Набережные Челны",
            "ООО \"РивалКом\", Республика Татарстан, г. Набережные Челны",
            "«Rosemount Inc.», США, Производственная площадка: Beijing Rosemount Far East Ins",
            "АО \"ПГ \"Метран\", г. Челябинск",
            "\"VEGA Grieshaber KG\", Германия",
            "Фирма \"ChangZhou XingYun Electronic Equipment Co., Ltd.\", КНР",
            "Фирма \"Shinko Denshi Co., Ltd.\", Япония",
            "ООО \"НПП \"Госметр\", г.С.-Петербург",
            "ООО \"Стандарт\"",
            "АО \"ИПФ \"Сибнефтеавтоматика\" (СибНА), г.Тюмень",
            "ЗАО \"ЭМИС\", г.Челябинск",
            "Фирма \"Emerson Process Management Flow BV\", Нидерланды; Фирма \"Emerson SRL\", Рум",
            "Фирма \"KROHNE Ltd.\", Великобритания",
            "Фирма \"Rota Yokogawa GmbH & Co. KG\", Германия",
            "ООО \"Компания Штрай\", г. Москва",
            "ООО «ЭлМетро Групп», г. Челябинск",
            "ООО \"ЭЛЬСТЕР Газэлектроника\"",
            "ООО «РАСКО Газэлектроника», г. Арзамас Нижегородской обл.",
            "Фирма \"Siemens SAS\", Франция; Фирма \"Huba Control AG\", Швейцария",
            "ООО НПП \"Элемер\",г. Зеленоград",
            "ОАО \"Манотомь\", г. Томск",
            "ООО \"ПОИНТ\", Беларусь, г.Полоцк",
            "ООО \"ИТеК ББМВ\", г. Челябинск",
            "АО «ПО Физтех», г. Томск",
            "ООО НПЦ \"Манометр\", Республика Мордовия, г. Саранск",
            "ООО \"Энергоприбор\", г.Пермь",
            "ООО \"ЭРИС\", г. Чайковский",
            "ООО «Пожгазприбор», г. Санкт-Петербург",
            "ЗАО \"Электронстандарт-Прибор\", г.С.-Петербург",
            "ФГУП \"Смоленское ПО \"Аналитприбор\", г.Смоленск",
            "ООО \"ЭМИ-Прибор,г. Санкт-Петербург",
            "ООО НПП \"ТЭК\" г.Томск",
            "Фирма \"Fluke Corporation\", США",
            "ООО \"Евромикс\", г.Москва",
            "ООО «БИОСОФТ-М», г. Москва",
            "ООО \"СКБ \"ПРОМАВТОМАТИКА\", г. Зеленоград",
            "ООО \"МИКОНТ\""
        };

        private static string[] uniqueUnits =
        {
            "МПа", "°С",
            "кгс/см2",
            "℃",
            "°C",
            "Атм",
            "кг/см2",
            "мм",
            "м",
            "% об.д.",
            "мг/м3",
            "% НКПР",
            "",
            "мм.рт.ст.",
            "мг/л",
            "Гц",
            "мА"
        };

        private static string[] customStatuses =
        {
            "На учатске",
            "В мастерской",
        };

        public static List<Equipment> GenereateEquipment(int quantity)
        {
            List<Equipment> randEquipmentList = [];
            for (int i = 0; i < quantity; i++)
            {
                var randEquipment = new Equipment();
                randEquipment.Limit = GenerateLimits();
                randEquipment.Status = "";
                randEquipment.Tags = [];
                randEquipment.AccuracyClass = random.Next(5).ToString();
                randEquipment.RegistrationNumber = GenerateSerialNumber();
                randEquipment.FactoryNumber = GenerateSerialNumber();
                randEquipment.Manufacturer = Manufacturers[random.Next(Manufacturers.Length - 1)];
                randEquipment.Name = GenerateName();
                randEquipment.Type = Types[random.Next(Types.Length - 1)];
                randEquipment.Units = uniqueUnits[random.Next(uniqueUnits.Length - 1)];
                randEquipment.Description = "";
                randEquipmentList.Add(randEquipment);
            }
            
            return randEquipmentList;
        }

        public static string GenerateLimits()
        {
            int floor = random.Next(1000);
            int ceiling;
            if (random.Next(1) == 1)
            {
                floor *= -1;
                if (random.Next(1) == 1)
                {
                    ceiling = -1 * random.Next(0,floor*-1);
                }
                else
                {
                    ceiling = random.Next(1000);
                }
            }
            else
            {
                ceiling = random.Next(floor,1000);
            }

            return floor.ToString()+".."+ceiling.ToString();
        }

        public static string GenerateSerialNumber(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GenerateNameNumber(int length = 10)
        {
            const string chars = "0123456789";

            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GenerateName()
        {
            return Names[random.Next(Names.Length - 1)] + GenerateNameNumber(random.Next(1,5));
        }
    }
}
