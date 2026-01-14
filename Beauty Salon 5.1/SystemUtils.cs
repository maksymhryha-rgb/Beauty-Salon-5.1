namespace Beauty_Salon_5._1
{
    static class SystemUtils
    {
        public static void CheckAndCreate(string path, string header)
        {
            if (!File.Exists(path))
            {
                // Створюємо масив з одним рядком і записуємо
                File.WriteAllLines(path, [header]);
            }
        }

        public static int GetNextId(string path)
        {
            if (!File.Exists(path)) return 1; // Якщо файлу нема, то ID буде 1

            var lines = File.ReadAllLines(path).Skip(1);
            int max = 0;

            foreach (var line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length > 0 && int.TryParse(parts[0], out int id))
                {
                    if (id > max) max = id;
                }
            }
            return max + 1;
        }
    }
}