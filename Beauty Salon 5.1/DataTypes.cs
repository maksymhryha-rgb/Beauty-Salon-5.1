namespace Beauty_Salon_5._1
{
    // Окремий файл для структур

    struct Service
    {
        public int Id;
        public string Name;
        public double Price;
    }

    // Структура для людей 
    struct Person
    {
        public int Id;
        public string Name;
        public string Role; // "Client" або "Master"
    }

    // Структура запису на прийом
    struct Appointment
    {
        public int Id;
        public int ClientId;
        public int StaffId;
        public List<int> ServiceIds; // Список ID послуг
        public DateTime Date;
    }
}