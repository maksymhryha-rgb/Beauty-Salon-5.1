using System.Text;

namespace Beauty_Salon_5._1
{
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            // Ініціалізація всіх файлів при старті
            ServiceModule.Init();
            ClientsAndStaff.Init();
            RecordsModule.Init();

            // Запуск системи авторизації
            if (AuthModule.AuthSystem())
            {
                MainMenu();
            }
        }

        static void MainMenu()
        {
            // Головний цикл програми
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

                // Викликаємо методи з відповідних модулів
                switch (Console.ReadLine())
                {
                    case "1": ServiceModule.Menu(); break;
                    case "2": ClientsAndStaff.MenuClients(); break;
                    case "3": ClientsAndStaff.MenuPersonnel(); break;
                    case "4": RecordsModule.Menu(); break;
                    case "5": RecordsModule.ViewReminders(); break;
                    case "0": return;
                }
            }
        }
    }
}