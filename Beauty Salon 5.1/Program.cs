using System.Text;
using System.Globalization;
using System.Security.Cryptography;

namespace BeautySalon
{
    struct Service
    {
        public int Id;
        public string Name;
        public double Price;
    }

    struct Person
    {
        public int Id;
        public string Name;
        public string Role;
    }

    struct Appointment
    {
        public int Id;
        public int ClientId;
        public int StaffId;
        public List<int> ServiceIds;
        public DateTime Date;
    }

    class Program
    {
        const string FileServices = "services.csv";
        const string FilePeople = "people.csv";
        const string FileRecords = "records.csv";
        const string FileUsers = "users.csv";

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            InitFiles();

            if (AuthSystem())
            {
                MainMenu();
            }
        }

        static void InitFiles()
        {
            CheckAndCreate(FileServices, "Id,Name,Price");
            CheckAndCreate(FilePeople, "Id,Name,Role");
            CheckAndCreate(FileRecords, "Id,Client,Staff,ServiceIds,Date");
            CheckAndCreate(FileUsers, "Id,Email,Pass");
        }

        static void CheckAndCreate(string path, string header)
        {
            if (!File.Exists(path))
            {
                File.WriteAllLines(path, new[] { header });
            }
        }

        static bool AuthSystem()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== ВХІД У СИСТЕМУ ===");
                Console.WriteLine("1. Вхід");
                Console.WriteLine("2. Реєстрація");
                Console.WriteLine("0. Вихід");
                Console.Write(">>> ");

                string key = Console.ReadLine();

                if (key == "1") { if (TryLogin()) return true; }
                else if (key == "2") TryRegister();
                else if (key == "0") Environment.Exit(0);
            }
        }

        static bool TryLogin()
        {
            Console.Write("Email: ");
            string email = Console.ReadLine();
            Console.Write("Пароль: ");
            string pass = Console.ReadLine();

            if (string.IsNullOrEmpty(pass)) return false;

            string hash = ComputeHash(pass);

            if (File.Exists(FileUsers))
            {
                var lines = File.ReadAllLines(FileUsers).Skip(1);
                foreach (var line in lines)
                {
                    var p = line.Split(',');
                    if (p.Length >= 3 && p[1] == email && p[2] == hash) return true;
                }
            }

            Console.WriteLine("Невірний логін або пароль!");
            Console.ReadKey();
            return false;
        }

        static void TryRegister()
        {
            Console.Write("Введіть Email: ");
            string email = Console.ReadLine();

            var lines = File.ReadAllLines(FileUsers).Skip(1);
            if (lines.Any(l => l.Split(',').Length > 1 && l.Split(',')[1] == email))
            {
                Console.WriteLine("Такий користувач вже існує.");
                Console.ReadKey();
                return;
            }

            Console.Write("Введіть пароль: ");
            string pass = Console.ReadLine();

            if (string.IsNullOrEmpty(pass))
            {
                Console.WriteLine("Пароль не може бути порожнім.");
                Console.ReadKey();
                return;
            }

            string hash = ComputeHash(pass);
            int id = GetNextId(FileUsers);

            File.AppendAllText(FileUsers, $"{id},{email},{hash}\n");
            Console.WriteLine("Реєстрація успішна.");
            Console.ReadKey();
        }

        static string ComputeHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes) builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        static void MainMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== ГОЛОВНЕ МЕНЮ ===");
                Console.WriteLine("1. Послуги");
                Console.WriteLine("2. Клієнти");
                Console.WriteLine("3. Персонал");
                Console.WriteLine("4. Записи");
                Console.WriteLine("5. Нагадування");
                Console.WriteLine("0. Вихід");
                Console.Write(">>> ");

                switch (Console.ReadLine())
                {
                    case "1": MenuServices(); break;
                    case "2": MenuClients(); break;
                    case "3": MenuPersonnel(); break;
                    case "4": MenuAppointments(); break;
                    case "5": ViewReminders(); break;
                    case "0": return;
                }
            }
        }

        static void MenuServices()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("--- ПОСЛУГИ ---");
                Console.WriteLine("1. Список послуг");
                Console.WriteLine("2. Додати послугу");
                Console.WriteLine("3. Видалити послугу");
                Console.WriteLine("4. Пошук послуги");
                Console.WriteLine("5. Статистика");
                Console.WriteLine("6. Редагувати послугу");
                Console.WriteLine("0. Назад");
                Console.Write(">>> ");

                switch (Console.ReadLine())
                {
                    case "1": ShowServicesTable(); Console.ReadKey(); break;
                    case "2": AddService(); break;
                    case "3": DeleteService(); break;
                    case "4": SearchService(); break;
                    case "5": ShowServiceStats(); break;
                    case "6": EditService(); break;
                    case "0": return;
                }
            }
        }

        static void ShowServicesTable()
        {
            var list = GetServices();
            if (list.Count == 0) Console.WriteLine("Список послуг порожній.");

            Console.WriteLine("{0,-5} | {1,-20} | {2,-10}", "ID", "Назва", "Ціна");
            Console.WriteLine(new string('-', 40));
            foreach (var s in list)
                Console.WriteLine("{0,-5} | {1,-20} | {2,-10}", s.Id, s.Name, s.Price);
        }

        static void AddService()
        {
            Console.Write("Назва послуги: "); string name = Console.ReadLine();
            Console.Write("Ціна: ");
            if (double.TryParse(Console.ReadLine(), out double price))
            {
                int id = GetNextId(FileServices);
                File.AppendAllText(FileServices, $"{id},{name},{price}\n");
                Console.WriteLine("Послугу додано.");
            }
            else Console.WriteLine("Помилка: Ціна має бути числом.");
            Console.ReadKey();
        }

        static void DeleteService()
        {
            ShowServicesTable();
            Console.Write("Введіть ID для видалення: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var list = GetServices();
                var item = list.FirstOrDefault(x => x.Id == id);
                if (item.Id != 0)
                {
                    list.Remove(item);
                    var lines = new List<string> { "Id,Name,Price" };
                    foreach (var s in list) lines.Add($"{s.Id},{s.Name},{s.Price}");
                    File.WriteAllLines(FileServices, lines.ToArray());
                    Console.WriteLine("Видалено успішно.");
                }
                else Console.WriteLine("ID не знайдено.");
            }
            Console.ReadKey();
        }

        static void EditService()
        {
            ShowServicesTable();
            Console.Write("Введіть ID послуги для зміни: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var list = GetServices();
                var index = list.FindIndex(s => s.Id == id);

                if (index != -1)
                {
                    Console.WriteLine($"Поточна: {list[index].Name} - {list[index].Price} грн");

                    Console.Write("Нова назва (Enter - залишити): ");
                    string name = Console.ReadLine();
                    string finalName = string.IsNullOrWhiteSpace(name) ? list[index].Name : name;

                    Console.Write("Нова ціна (Enter - залишити): ");
                    string priceStr = Console.ReadLine();
                    double finalPrice = list[index].Price;
                    if (double.TryParse(priceStr, out double p)) finalPrice = p;

                    list[index] = new Service { Id = id, Name = finalName, Price = finalPrice };

                    var lines = new List<string> { "Id,Name,Price" };
                    foreach (var s in list) lines.Add($"{s.Id},{s.Name},{s.Price}");
                    File.WriteAllLines(FileServices, lines.ToArray());

                    Console.WriteLine("Зміни збережено.");
                }
                else Console.WriteLine("ID не знайдено.");
            }
            Console.ReadKey();
        }

        static void SearchService()
        {
            Console.Write("Введіть частину назви: ");
            string query = Console.ReadLine().ToLower();
            var list = GetServices().Where(s => s.Name.ToLower().Contains(query)).ToList();

            if (list.Count == 0) Console.WriteLine("Нічого не знайдено.");
            else
            {
                Console.WriteLine("Результати:");
                foreach (var s in list) Console.WriteLine($"#{s.Id} {s.Name} - {s.Price} грн");
            }
            Console.ReadKey();
        }

        static void ShowServiceStats()
        {
            var list = GetServices();
            if (list.Count == 0) { Console.WriteLine("Даних немає."); Console.ReadKey(); return; }

            Console.WriteLine($"Кількість: {list.Count}");
            Console.WriteLine($"Мін. ціна: {list.Min(x => x.Price)}");
            Console.WriteLine($"Макс. ціна: {list.Max(x => x.Price)}");
            Console.WriteLine($"Сума: {list.Sum(x => x.Price)}");
            Console.WriteLine($"Середня: {list.Average(x => x.Price):F2}");
            Console.ReadKey();
        }

        static void MenuClients()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("--- КЛІЄНТИ ---");
                Console.WriteLine("1. Список клієнтів");
                Console.WriteLine("2. Додати клієнта");
                Console.WriteLine("3. Видалити клієнта");
                Console.WriteLine("0. Назад");
                Console.Write(">>> ");

                string choice = Console.ReadLine();

                if (choice == "1")
                {
                    var list = GetPeople().Where(p => p.Role == "Client").ToList();
                    if (list.Count == 0) Console.WriteLine("Список порожній.");
                    else foreach (var p in list) Console.WriteLine($"ID: {p.Id} | {p.Name}");
                    Console.ReadKey();
                }
                else if (choice == "2")
                {
                    Console.Write("ПІБ Клієнта: ");
                    string name = Console.ReadLine();
                    int id = GetNextId(FilePeople);
                    File.AppendAllText(FilePeople, $"{id},{name},Client\n");
                    Console.WriteLine("Клієнта додано.");
                    Console.ReadKey();
                }
                else if (choice == "3")
                {
                    DeletePerson("Client");
                }
                else if (choice == "0") return;
            }
        }

        static void MenuPersonnel()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("--- ПЕРСОНАЛ (МАЙСТРИ) ---");
                Console.WriteLine("1. Список майстрів");
                Console.WriteLine("2. Додати майстра");
                Console.WriteLine("3. Видалити майстра");
                Console.WriteLine("0. Назад");
                Console.Write(">>> ");

                string choice = Console.ReadLine();

                if (choice == "1")
                {
                    var list = GetPeople().Where(p => p.Role == "Master").ToList();
                    if (list.Count == 0) Console.WriteLine("Список порожній.");
                    else foreach (var p in list) Console.WriteLine($"ID: {p.Id} | {p.Name}");
                    Console.ReadKey();
                }
                else if (choice == "2")
                {
                    Console.Write("ПІБ Майстра: ");
                    string name = Console.ReadLine();
                    int id = GetNextId(FilePeople);
                    File.AppendAllText(FilePeople, $"{id},{name},Master\n");
                    Console.WriteLine("Майстра додано.");
                    Console.ReadKey();
                }
                else if (choice == "3")
                {
                    DeletePerson("Master");
                }
                else if (choice == "0") return;
            }
        }

        static void DeletePerson(string roleFilter)
        {
            var allPeople = GetPeople();
            var targetList = allPeople.Where(p => p.Role == roleFilter).ToList();

            if (targetList.Count == 0)
            {
                Console.WriteLine($"Список ({roleFilter}) порожній.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"--- Видалення ({roleFilter}) ---");
            foreach (var p in targetList) Console.WriteLine($"ID: {p.Id} | {p.Name}");

            Console.Write("Введіть ID для видалення: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var personToDelete = allPeople.FirstOrDefault(p => p.Id == id && p.Role == roleFilter);

                if (personToDelete.Id != 0)
                {
                    allPeople.Remove(personToDelete);

                    var lines = new List<string> { "Id,Name,Role" };
                    foreach (var p in allPeople) lines.Add($"{p.Id},{p.Name},{p.Role}");

                    File.WriteAllLines(FilePeople, lines.ToArray());
                    Console.WriteLine("Видалено успішно.");
                }
                else
                {
                    Console.WriteLine("ID не знайдено або це не та роль.");
                }
            }
            else
            {
                Console.WriteLine("Невірний формат ID.");
            }
            Console.ReadKey();
        }

        static void MenuAppointments()
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

        static void AddAppointment()
        {
            Console.Clear();
            var people = GetPeople();
            var clients = people.Where(p => p.Role == "Client").ToList();
            var masters = people.Where(p => p.Role == "Master").ToList();
            var services = GetServices();

            if (!clients.Any() || !masters.Any() || !services.Any())
            {
                Console.WriteLine("Помилка: База даних неповна.");
                Console.ReadKey(); return;
            }

            Console.WriteLine("=== НОВИЙ ЗАПИС ===");

            Console.WriteLine("\nКлієнти:");
            foreach (var c in clients) Console.WriteLine($"{c.Id}. {c.Name}");
            int cId = GetValidId("ID Клієнта", clients.Select(x => x.Id).ToList());

            Console.WriteLine("\nМайстри:");
            foreach (var m in masters) Console.WriteLine($"{m.Id}. {m.Name}");
            int mId = GetValidId("ID Майстра", masters.Select(x => x.Id).ToList());

            Console.WriteLine("\nПослуги:");
            foreach (var s in services) Console.WriteLine($"{s.Id}. {s.Name} ({s.Price} грн)");

            List<int> selectedServices = new List<int>();
            double currentTotal = 0;

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
                int id = GetNextId(FileRecords);
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

            var people = GetPeople();
            var services = GetServices();

            Console.WriteLine("{0,-16} | {1,-10} | {2,-10} | {3,-25} | {4}", "Час", "Клієнт", "Майстер", "Послуги", "СУМА");
            Console.WriteLine(new string('-', 90));

            foreach (var a in apps.OrderBy(x => x.Date))
            {
                string c = people.FirstOrDefault(p => p.Id == a.ClientId).Name ?? "?";
                string m = people.FirstOrDefault(p => p.Id == a.StaffId).Name ?? "?";

                List<string> sNames = new List<string>();
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
                if (servicesStr.Length > 25) servicesStr = servicesStr.Substring(0, 22) + "...";

                Console.WriteLine($"{a.Date:dd.MM HH:mm}      | {c,-10} | {m,-10} | {servicesStr,-25} | {totalCheck} грн");
            }
            Console.ReadKey();
        }

        static void ViewReminders()
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
                var people = GetPeople();
                foreach (var a in apps)
                {
                    string cName = people.FirstOrDefault(p => p.Id == a.ClientId).Name ?? "Клієнт";
                    Console.WriteLine($"{a.Date:dd.MM.yyyy HH:mm} - {cName} (ID: {a.ClientId})");
                }
            }
            Console.ReadKey();
        }

        static List<Service> GetServices()
        {
            var list = new List<Service>();
            if (!File.Exists(FileServices)) return list;

            foreach (var line in File.ReadAllLines(FileServices).Skip(1))
            {
                var p = line.Split(',');
                if (p.Length >= 3 && int.TryParse(p[0], out int id) && double.TryParse(p[2], out double price))
                {
                    list.Add(new Service { Id = id, Name = p[1], Price = price });
                }
            }
            return list;
        }

        static List<Person> GetPeople()
        {
            var list = new List<Person>();
            if (!File.Exists(FilePeople)) return list;

            foreach (var line in File.ReadAllLines(FilePeople).Skip(1))
            {
                var p = line.Split(',');
                if (p.Length >= 3 && int.TryParse(p[0], out int id))
                {
                    list.Add(new Person { Id = id, Name = p[1], Role = p[2] });
                }
            }
            return list;
        }

        static List<Appointment> GetAppointments()
        {
            var list = new List<Appointment>();
            if (!File.Exists(FileRecords)) return list;

            foreach (var line in File.ReadAllLines(FileRecords).Skip(1))
            {
                var p = line.Split(',');
                if (p.Length >= 5 && int.TryParse(p[0], out int id) &&
                    DateTime.TryParseExact(p[4], "dd.MM.yyyy HH:mm", null, DateTimeStyles.None, out DateTime dt))
                {
                    List<int> sIds = new List<int>();
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

        static int GetNextId(string path)
        {
            if (!File.Exists(path)) return 1;
            var lines = File.ReadAllLines(path).Skip(1);
            int max = 0;
            foreach (var line in lines)
            {
                var p = line.Split(',');
                if (p.Length > 0 && int.TryParse(p[0], out int id))
                {
                    if (id > max) max = id;
                }
            }
            return max + 1;
        }
    }
}