using EMAS.Model;

namespace DataBaseManipulator.Factory
{
    public static class LocationFactory
    {
        private static string[] Names =
        {
            "Тымпучиканский участок недр",
            "Вакунайский участок",
            "БНУ",
            "Лукойл Пякяхинское",
            "Семаковское месторождение",
            "Чаяндинское НГКМ",
            "Новопортовское ГКМ",
            "Ачим Девелопмент",
            "Заполярное НГКМ",
            "ГДУ",
            "ПКИОС 66",
            "Бованенково",
            "АчимГаз",
            "ПКИОС 6",
            "ПКИОС 2",
            "ПКИОС 3",
            "ПКИОС 5",
            "ПКИОС 1",
            "ПКИОС 39",
            "БКМ № 0053",
            "МЗС",
            "ПКИОС 4",
            "БК №1",
            "БКМ № 2",
            "БКМ2 № 3",
            "БК2 №5",
            "МТСУ № 250",
            "МТСУ № 331",
            "МТСУ № 487",
            "МТСУ № 477",
            "МТСУ № 478",
            "МТСУ № 480",
            "МУИС-1",
            "УПВ с БКНС"
        };

        public static Location[] GetLocations(int numberOfLocations = 0)
        {
            if (numberOfLocations == 0)
            {
                numberOfLocations = Names.Length;
            }

            Location[] locations = new Location[numberOfLocations];
            for (int i = 0; i < numberOfLocations; i++)
            {
                locations[i] = new Location(0, Names[i]);
            }
            return locations;
        }

    }
}
