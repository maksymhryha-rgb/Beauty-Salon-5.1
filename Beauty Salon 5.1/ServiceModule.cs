namespace Beauty_Salon_5._1
{
    static class ServiceModule
    {
        const string FileServices = "services.csv";

        public static void Init()
        {
            SystemUtils.CheckAndCreate(FileServices, "Id,Name,Price");
        }

        public static void Menu()
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

        public static List<Service> GetServices()
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
            Console.Write("Назва послуги: ");
            string name = Console.ReadLine();

            // Валідація
            if (string.IsNullOrWhiteSpace(name) || name.Length < 3 || name.Length > 30)
            {
                Console.WriteLine("Помилка: Назва має бути від 3 до 30 символів.");
                Console.ReadKey();
                return;
            }

            Console.Write("Ціна: ");
            if (double.TryParse(Console.ReadLine(), out double price))
            {
                if (price <= 0 || price > 50000)
                {
                    Console.WriteLine("Помилка: Ціна має бути в межах від 1 до 50000.");
                    Console.ReadKey();
                    return;
                }

                int id = SystemUtils.GetNextId(FileServices);
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
                    // Перезаписуємо файл повністю без видаленого елемента
                    var lines = new List<string> { "Id,Name,Price" };
                    foreach (var s in list) lines.Add($"{s.Id},{s.Name},{s.Price}");
                    File.WriteAllLines(FileServices, [.. lines]);
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

                    // Збереження змін
                    var lines = new List<string> { "Id,Name,Price" };
                    foreach (var s in list) lines.Add($"{s.Id},{s.Name},{s.Price}");
                    File.WriteAllLines(FileServices, [.. lines]);

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
            var list = GetServices().Where(s => s.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase)).ToList();

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

            Console.WriteLine($"Кількість послуг: {list.Count}");
            Console.WriteLine($"Мін. ціна: {list.Min(x => x.Price)}");
            Console.WriteLine($"Макс. ціна: {list.Max(x => x.Price)}");
            Console.WriteLine($"Сума: {list.Sum(x => x.Price)}");
            Console.WriteLine($"Середня: {list.Average(x => x.Price):F2}");

            Console.WriteLine();
            Console.WriteLine("--- Рейтинг популярності послуг ---");

            var appointments = RecordsModule.GetAppointments();

            var statQuery = appointments
                .SelectMany(a => a.ServiceIds)
                .GroupBy(id => id)
                .Select(g => new { ServiceId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count);

            if (!statQuery.Any())
            {
                Console.WriteLine("Немає даних про замовлення для формування рейтингу.");
            }
            else
            {
                foreach (var item in statQuery)
                {
                    var serviceName = list.FirstOrDefault(s => s.Id == item.ServiceId).Name;
                    if (!string.IsNullOrEmpty(serviceName))
                    {
                        Console.WriteLine($"Послуга '{serviceName}': замовлено {item.Count} раз(ів)");
                    }
                }
            }
            Console.ReadKey();
        }
    }
}