using System.Security.Cryptography;
using System.Text;

namespace Beauty_Salon_5._1
{
    // Модуль авторизації 
    static class AuthModule
    {
        const string FileUsers = "users.csv"; // Файл з паролями

        public static bool AuthSystem()
        {
            // Ініціалізація файлу юзерів
            SystemUtils.CheckAndCreate(FileUsers, "Id,Email,Pass");

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== ВХІД У СИСТЕМУ ===");
                Console.WriteLine("1. Вхід");
                Console.WriteLine("2. Реєстрація");
                Console.WriteLine("0. Вихід");
                Console.Write(">>> ");

                string key = Console.ReadLine();

                if (key == "1")
                {
                    if (TryLogin()) return true;
                }
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

            // Перевірка на дублікати
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
            int id = SystemUtils.GetNextId(FileUsers);

            File.AppendAllText(FileUsers, $"{id},{email},{hash}\n");
            Console.WriteLine("Реєстрація успішна.");
            Console.ReadKey();
        }

        static string ComputeHash(string input)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            StringBuilder builder = new();
            foreach (var b in bytes) builder.Append(b.ToString("x2"));
            return builder.ToString();
        }
    }
}