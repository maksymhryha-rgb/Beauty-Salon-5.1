using System.Globalization;

namespace Beauty_Salon_5._1
{
    // Модуль записів
    static class RecordsModule
    {
        const string FileRecords = "records.csv";

        public static void Init()
        {
            SystemUtils.CheckAndCreate(FileRecords, "Id,Client,Staff,ServiceIds,Date");
        }

        public static void Menu()
        {
            Console.Clear();
            Console.WriteLine("--- ЗАПИСИ ---");
            Console.WriteLine("1. Створити запис");
            Console.WriteLine("2. Журнал");
            Console.WriteLine("0. Назад");

            string choice = Console.ReadLine();

            if (choice == "1") AddAppointment();
            else if (choice == "2") ViewAppointments();
        }

        public static List<Appointment> GetAppointments()
        {
            var list = new List<Appointment>();
            if (!File.Exists(FileRecords)) return list;

            foreach (var line in File.ReadAllLines(FileRecords).Skip(1))
            {
                var p = line.Split(',');
                if (p.Length >= 5 && int.TryParse(p[0], out int id) &&
                    DateTime.TryParseExact(p[4], "dd.MM.yyyy HH:mm", null, DateTimeStyles.None, out DateTime dt))
                {
                    List<int> sIds = [];
                    if (!string.IsNullOrEmpty(p[3]))
                    {
                        foreach (var sPart in p[3].Split('|'))
                        {
                            if (int.TryParse(sPart, out int sId)) sIds.Add(sId);
                        }
                    }

                    list.Add(new Appointment
                    {
                        Id = id,
                        ClientId = int.Parse(p[1]),
                        StaffId = int.Parse(p[2]),
                        ServiceIds = sIds,
                        Date = dt
                    });
                }
            }
            return list;
        }

        static void AddAppointment()
        {
            Console.Clear();
            // Підтягуємо дані з сусідніх модулів
            var people = ClientsAndStaff.GetPeople();
            var clients = people.Where(p => p.Role == "Client").ToList();
            var masters = people.Where(p => p.Role == "Master").ToList();
            var services = ServiceModule.GetServices();

            if (clients.Count == 0 || masters.Count == 0 || services.Count == 0)
            {
                Console.WriteLine("Помилка: База даних неповна (немає клієнтів, майстрів або послуг).");
                Console.ReadKey(); return;
            }

            Console.WriteLine("=== НОВИЙ ЗАПИС ===");

            Console.WriteLine("\nКлієнти:");
            foreach (var c in clients) Console.WriteLine($"{c.Id}. {c.Name}");
            int cId = GetValidId("ID Клієнта", [.. clients.Select(x => x.Id)]);

            Console.WriteLine("\nМайстри:");
            foreach (var m in masters) Console.WriteLine($"{m.Id}. {m.Name}");
            int mId = GetValidId("ID Майстра", [.. masters.Select(x => x.Id)]);

            Console.WriteLine("\nПослуги:");
            foreach (var s in services) Console.WriteLine($"{s.Id}. {s.Name} ({s.Price} грн)");

            List<int> selectedServices = [];
            double currentTotal = 0;

            // Цикл вибору послуг
            while (true)
            {
                Console.Write("\nВведіть ID послуги (або 0 щоб завершити): ");
                string input = Console.ReadLine();
                if (input == "0")
                {
                    if (selectedServices.Count == 0)
                    {
                        Console.WriteLine("Потрібно обрати хоча б одну послугу!");
                        continue;
                    }
                    break;
                }

                if (int.TryParse(input, out int sId) && services.Any(s => s.Id == sId))
                {
                    selectedServices.Add(sId);
                    var s = services.First(x => x.Id == sId);
                    currentTotal += s.Price;
                    Console.WriteLine($"-> Додано: {s.Name}. Поточна сума: {currentTotal} грн");
                }
                else
                {
                    Console.WriteLine("Невірний ID послуги.");
                }
            }

            Console.Write("\nДата (дд.мм.рррр гг:хх): ");
            if (DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy HH:mm", null, DateTimeStyles.None, out DateTime dt))
            {
                int id = SystemUtils.GetNextId(FileRecords);
                string servicesString = string.Join("|", selectedServices);

                File.AppendAllText(FileRecords, $"{id},{cId},{mId},{servicesString},{dt:dd.MM.yyyy HH:mm}\n");

                Console.WriteLine("\n---------------------------");
                Console.WriteLine("ЗАПИС УСПІШНО СТВОРЕНО!");
                Console.WriteLine($"ВСЬОГО ДО СПЛАТИ: {currentTotal} грн");
                Console.WriteLine("---------------------------");
            }
            else Console.WriteLine("Невірний формат дати.");
            Console.ReadKey();
        }

        static int GetValidId(string prompt, List<int> validIds)
        {
            while (true)
            {
                Console.Write($"{prompt}: ");
                if (int.TryParse(Console.ReadLine(), out int id) && validIds.Contains(id)) return id;
                Console.WriteLine("Невірний ID.");
            }
        }

        static void ViewAppointments()
        {
            Console.Clear();
            var apps = GetAppointments();
            if (apps.Count == 0) { Console.WriteLine("Журнал порожній."); Console.ReadKey(); return; }

            var people = ClientsAndStaff.GetPeople();
            var services = ServiceModule.GetServices();

            Console.WriteLine("{0,-16} | {1,-10} | {2,-10} | {3,-25} | {4}", "Час", "Клієнт", "Майстер", "Послуги", "СУМА");
            Console.WriteLine(new string('-', 90));

            foreach (var a in apps.OrderBy(x => x.Date))
            {
                string c = people.FirstOrDefault(p => p.Id == a.ClientId).Name ?? "?";
                string m = people.FirstOrDefault(p => p.Id == a.StaffId).Name ?? "?";

                List<string> sNames = [];
                double totalCheck = 0;

                foreach (int sId in a.ServiceIds)
                {
                    var s = services.FirstOrDefault(x => x.Id == sId);
                    if (s.Id != 0)
                    {
                        sNames.Add(s.Name);
                        totalCheck += s.Price;
                    }
                }
                string servicesStr = string.Join(", ", sNames);
                if (servicesStr.Length > 25) servicesStr = servicesStr[..22] + "...";

                Console.WriteLine($"{a.Date:dd.MM HH:mm}       | {c,-10} | {m,-10} | {servicesStr,-25} | {totalCheck} грн");
            }
            Console.ReadKey();
        }

        public static void ViewReminders()
        {
            Console.Clear();
            Console.WriteLine("=== МАЙБУТНІ ВІЗИТИ ===");

            var apps = GetAppointments()
                .Where(x => x.Date >= DateTime.Now)
                .OrderBy(x => x.Date)
                .ToList();

            if (apps.Count == 0) Console.WriteLine("Майбутніх записів немає.");
            else
            {
                var people = ClientsAndStaff.GetPeople();
                foreach (var a in apps)
                {
                    string cName = people.FirstOrDefault(p => p.Id == a.ClientId).Name ?? "Клієнт";
                    Console.WriteLine($"{a.Date:dd.MM.yyyy HH:mm} - {cName} (ID: {a.ClientId})");
                }
            }
            Console.ReadKey();
        }
    }
}