namespace Beauty_Salon_5._1
{
    // Модуль для роботи з людьми (і клієнти і персонал)
    static class ClientsAndStaff
    {
        const string FilePeople = "people.csv";

        public static void Init()
        {
            SystemUtils.CheckAndCreate(FilePeople, "Id,Name,Role");
        }

        // Публічний метод для отримання списку людей 
        public static List<Person> GetPeople()
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

        public static void MenuClients()
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
                    AddPerson("Client");
                }
                else if (choice == "3")
                {
                    DeletePerson("Client");
                }
                else if (choice == "0") return;
            }
        }

        public static void MenuPersonnel()
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
                    AddPerson("Master");
                }
                else if (choice == "3")
                {
                    DeletePerson("Master");
                }
                else if (choice == "0") return;
            }
        }

        // Універсальний метод додавання
        static void AddPerson(string role)
        {
            Console.Write($"Введіть ПІБ ({role}): ");
            string name = Console.ReadLine();
            int id = SystemUtils.GetNextId(FilePeople);
            File.AppendAllText(FilePeople, $"{id},{name},{role}\n");
            Console.WriteLine("Додано успішно.");
            Console.ReadKey();
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

                    File.WriteAllLines(FilePeople, [.. lines]);
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
    }
}