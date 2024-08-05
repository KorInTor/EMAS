using DataBaseManipulator.Factory;
using Model.Exceptions;
using Model;
using Service.Connection;
using Npgsql;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        MainMenu();
    }

    private static void MainMenu()
    {
        try
        {
            Console.Write("Trying to connect as PDS");
            Task task = Task.Run(() => SessionManager.Login("Пряхин", "ps123123"));
            EmulateBusyness(task);
        }
        catch (ConnectionFailedException)
        {
            Console.WriteLine("Server connection failed, terminating.");
            return;
        }
        do
        {
            Console.Clear();
            Console.WriteLine("Choose SubMenu:");
            Console.WriteLine("3 - Equipment.");
            Console.WriteLine("or:");
            Console.WriteLine("C - Clear all Tables.");
            char command;
            command = Console.ReadKey().KeyChar;
            Console.WriteLine();

            switch (command)
            {
                case '1':
                    {
                        break;
                    }
                case '2':
                    {
                        break;
                    }
                case '3':
                    {
                        EquipmentMenu();
                        break;
                    }
                case 'C':
                    {
                        ClearTable("public.\"event\"");
                        ClearTable("public.\"storable_object\"");
                        break;
                    }
                default:
                    {
                        Console.WriteLine("!Wrong command!");
                        break;
                    }
            }
        } while (true);
    }

    private static void EquipmentMenu()
    {
        bool exitFlag = false;
        do
        {
            Console.Clear();

            Console.WriteLine("0 - Generate list of Materials and add to DB");
            Console.WriteLine("1 - Generate and Insert to DB.");
            Console.WriteLine("2 - Truncate(Clear All Info).");
            Console.WriteLine("3 - Emulate events. (Creates history entrys)");
            Console.WriteLine("M - Go Back To Main");
            char command;
            command = Console.ReadKey().KeyChar;
            Console.WriteLine();

            switch (command)
            {
                case '0':
                    {
                        Console.WriteLine("Quantity (default = 100): ");
                        if (!int.TryParse(Console.ReadLine(), out int quantity))
                            quantity = 100;

                        List<MaterialPiece> randMaterialsList = [];
                        Console.WriteLine("Generating List");
                        Task task = Task.Run(() => { randMaterialsList = MaterialsFactory.GenerateMaterials(quantity);});
                        EmulateBusyness(task);

                        Console.Write("Getting locationId Ids");

                        Dictionary<int, string> namedLocations = [];
                        task = Task.Run(() => namedLocations = DataBaseClient.GetInstance().SelectNamedLocations());
                        EmulateBusyness(task);

                        Console.WriteLine("Sending Materials");
                        task = Task.Run(() => DistributeMaterialsListEvenlyOnLocatinos(randMaterialsList, namedLocations));
                        while (!task.IsCompleted)
                        {
                            Console.WriteLine($"{randMaterialsList.Count} Left.");
                            Thread.Sleep(500);
                        }

                        Console.Write("");
                        break;
                    }
                case '1':
                    {
                        Console.Write("Quantity (default = 100): ");
                        if (!int.TryParse(Console.ReadLine(), out int quantity))
                            quantity = 100;

                        List<Equipment> randEquipmentList = [];
                        Console.Write("GeneratingList");
                        Task task = Task.Run(() => randEquipmentList = EquipmentFactory.GenereateEquipment(quantity));
                        EmulateBusyness(task);

                        Console.Write("Getting locationId Ids");

                        Dictionary<int, string> namedLocations = [];
                        task = Task.Run(() => namedLocations = DataBaseClient.GetInstance().SelectNamedLocations());
                        EmulateBusyness(task);

                        Console.WriteLine("Sending the Equipment");
                        task = Task.Run(() => DistributeEquipmentListEvenlyOnLocatinos(randEquipmentList, namedLocations));
                        while (!task.IsCompleted)
                        {
                            Console.WriteLine($"{randEquipmentList.Count} Left.");
                            Thread.Sleep(500);
                        }

                        Console.Write("");
                        break;
                    }
                case '2':
                    {
                        ClearTable("public.storable_object");
                        ClearTable("public.\"event\"");
                        RestartSequence("public.storable_object_id_seq");
                        break;
                    }
                case '3':
                    {
                        throw new NotImplementedException();
                        break;
                    }
                case 'M':
                    {
                        exitFlag = true;
                        break;
                    }
                default:
                    {
                        Console.WriteLine("!Wrong command!");
                        break;
                    }
            }
        } while (!exitFlag);
    }

    private static void RestartSequence(string seqName)
    {
        var connection = ConnectionPool.GetConnection();

        string query = ("ALTER SEQUENCE "+ seqName +" RESTART WITH 1;");

        using (var command = new NpgsqlCommand(query, connection))
            command.ExecuteNonQuery();

        ConnectionPool.ReleaseConnection(connection);
    }

    private static void DistributeEquipmentListEvenlyOnLocatinos(List<Equipment> randEquipmentList, Dictionary<int, string> namedLocations)
    {
        foreach (int locationId in namedLocations.Keys)
        {
            int remaining = randEquipmentList.Count / namedLocations.Keys.Count;
            while (remaining != 0)
            {
                DataBaseClient.GetInstance().Add(randEquipmentList.Last(), locationId);
                randEquipmentList.RemoveAt(randEquipmentList.Count - 1);
                remaining -= 1;
            }
        }

        if (randEquipmentList.Count > 0)
        {
            int remaining = randEquipmentList.Count;
            while (remaining != 0)
            {
                DataBaseClient.GetInstance().Add(randEquipmentList.Last(), namedLocations.Keys.Last());
                randEquipmentList.RemoveAt(randEquipmentList.Count - 1);
                remaining -= 1;
            }
        }
    }

    private static void DistributeMaterialsListEvenlyOnLocatinos(List<MaterialPiece> randMaterialsList, Dictionary<int, string> namedLocations)
    {
        foreach (int locationId in namedLocations.Keys)
        {
            int remaining = randMaterialsList.Count / namedLocations.Keys.Count;
            while (remaining != 0)
            {
                DataBaseClient.GetInstance().Add(randMaterialsList.Last(), locationId);
                randMaterialsList.RemoveAt(randMaterialsList.Count - 1);
                remaining -= 1;
            }
        }

        if (randMaterialsList.Count > 0)
        {
            int remaining = randMaterialsList.Count;
            while (remaining != 0)
            {
                DataBaseClient.GetInstance().Add(randMaterialsList.Last(), namedLocations.Keys.Last());
                randMaterialsList.RemoveAt(randMaterialsList.Count - 1);
                remaining -= 1;
            }
        }
    }

    /// <summary>
    /// For some unrevield reasons this throws exception saying some sequence is empty, bc fuck you and everything around you
    /// </summary>
    /// <param name="randMaterialsList"></param>
    /// <param name="locations"></param>
    private static void DistributeMaterialsEvenlyOnLocations(List<MaterialPiece> randMaterialsList, Dictionary<int, string> locations)
    {
        int materialPiecesToLocationsRatio = randMaterialsList.Count / locations.Count;

        foreach(int locationId in locations.Keys)
        {
            for (int i = 0; i < materialPiecesToLocationsRatio; i++)
            {
                DataBaseClient.GetInstance().Add(randMaterialsList.Last(), locationId);// <-- Here
                randMaterialsList.Remove(randMaterialsList.Last());
            }
            if (randMaterialsList.Count > materialPiecesToLocationsRatio + 1)
                materialPiecesToLocationsRatio++;
        }
    }

    private static void EmulateBusyness(Task task)
    {
        while (!task.IsCompleted)
        {
            Console.Write(".");
            Thread.Sleep(500);
        }
        Console.WriteLine("OK");
    }

    private static void ClearTable(string tableName)
    {
        var connection = ConnectionPool.GetConnection();

        string query = ("TRUNCATE " + tableName + " RESTART IDENTITY CASCADE");

        using (var command = new NpgsqlCommand(query, connection))
            command.ExecuteNonQuery();

        ConnectionPool.ReleaseConnection(connection);
    } 
}