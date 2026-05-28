using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab10
{
    public class FacultyEventArgs : EventArgs
    {
        public string EventName { get; }
        public string Location { get; }
        public int Day { get; }
        // ЗАДАЧА 2: Пріоритет
        public int Priority { get; }
        public string Result { get; set; }

        public FacultyEventArgs(string name, string location, int day, int priority)
        {
            EventName = name;
            Location = location;
            Day = day;
            Priority = priority;
            Result = "Очікує обробки";
        }
    }

    public delegate void FacultyEventHandler(object sender, FacultyEventArgs e);

    // ЗАДАЧА 1.3: Життя факультету (різні події)
    public class Faculty
    {
        private readonly string _name;
        private readonly int _daysToSimulate;
        private readonly Random _rnd = new Random();

        public event FacultyEventHandler FacultyDay;
        public event FacultyEventHandler ExamSession;
        public event FacultyEventHandler Conference;

        // ЗАДАЧА 2: Черги та статистика
        private readonly List<FacultyEventArgs> _eventQueue = new List<FacultyEventArgs>();
        private readonly List<FacultyEventArgs> _statistics = new List<FacultyEventArgs>();

        public Faculty(string name, int days)
        {
            _name = name;
            _daysToSimulate = days;
        }

        // ЗАДАЧА 2: Асинхронність
        public async Task SimulateAsync()
        {
            Console.WriteLine($"=== Початок симуляції факультету '{_name}' на {_daysToSimulate} днів ===");

            for (int day = 1; day <= _daysToSimulate; day++)
            {
                Console.WriteLine($"\n[День {day}] Ранкове планування подій...");

                GenerateRandomEvents(day);

                var sortedEvents = _eventQueue.OrderByDescending(e => e.Priority).ToList();
                _eventQueue.Clear();

                foreach (var ev in sortedEvents)
                {
                    await Task.Delay(300);
                    RaiseEvent(ev);
                    _statistics.Add(ev);
                }
            }

            DisplayStatistics();
        }

        private void GenerateRandomEvents(int day)
        {
            if (_rnd.Next(1, 100) <= 40)
                _eventQueue.Add(new FacultyEventArgs("День Факультету", "Головний актовий зал", day, 2));

            if (_rnd.Next(1, 100) <= 50)
                _eventQueue.Add(new FacultyEventArgs("Екзаменаційна сесія", "Аудиторія 404", day, 3));

            if (_rnd.Next(1, 100) <= 30)
                _eventQueue.Add(new FacultyEventArgs("Наукова конференція", "Конференц-зал", day, 1));
        }

        private void RaiseEvent(FacultyEventArgs e)
        {
            Console.WriteLine($"⚡ Подія: {e.EventName} (Пріоритет: {e.Priority}) у локації '{e.Location}'");

            switch (e.EventName)
            {
                case "День Факультету":
                    FacultyDay?.Invoke(this, e);
                    break;
                case "Екзаменаційна сесія":
                    ExamSession?.Invoke(this, e);
                    break;
                case "Наукова конференція":
                    Conference?.Invoke(this, e);
                    break;
            }
        }

        private void DisplayStatistics()
        {
            Console.WriteLine("\n==================================================");
            Console.WriteLine($"📊 СТАТИСТИКА ЗА ПЕРІОД СИМУЛЯЦІЇ ({_daysToSimulate} днів):");
            Console.WriteLine("==================================================");
            Console.WriteLine($"Всього подій відбулося: {_statistics.Count}");
            Console.WriteLine($"- Сесій: {_statistics.Count(s => s.EventName == "Екзаменаційна сесія")}");
            Console.WriteLine($"- Днів факультету: {_statistics.Count(s => s.EventName == "День Факультету")}");
            Console.WriteLine($"- Конференцій: {_statistics.Count(s => s.EventName == "Наукова конференція")}");
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("Журнал виконаних рішень підрозділами:");
            
            foreach (var item in _statistics)
            {
                Console.WriteLine($"День {item.Day} | {item.EventName} -> {item.Result}");
            }
            Console.WriteLine("==================================================\n");
        }
    }

    public class Dean
    {
        public void HandleFacultyDay(object sender, FacultyEventArgs e)
        {
            e.Result = "Деканат погодив вихідний та виділив кошти на призи.";
            Console.WriteLine($"   ↳ [Деканат]: {e.Result}");
        }

        public void HandleSession(object sender, FacultyEventArgs e)
        {
            e.Result = "Деканат підписав відомості та затвердив розклад комісій.";
            Console.WriteLine($"   ↳ [Деканат]: {e.Result}");
        }
    }

    public class SecurityService
    {
        public void HandleGenericEvent(object sender, FacultyEventArgs e)
        {
            Console.WriteLine($"   ↳ [Безпека]: Посилено охорону об'єкта '{e.Location}' на час заходу.");
        }
    }

    public class Lab10T2
    {
        public async Task RunAsync()
        {
            Faculty faculty = new Faculty("Комп'ютерних наук та ІТ", 5);
            Dean dean = new Dean();
            SecurityService security = new SecurityService();

            faculty.FacultyDay += dean.HandleFacultyDay;
            faculty.FacultyDay += security.HandleGenericEvent;

            faculty.ExamSession += dean.HandleSession;
            
            faculty.Conference += security.HandleGenericEvent;
            faculty.Conference += (sender, e) => {
                e.Result = "Кафедра зареєструвала закордонних гостей.";
                Console.WriteLine($"   ↳ [Кафедра]: {e.Result}");
            };

            await faculty.SimulateAsync();
        }
    }

    // ТОЧКА ВХОДУ В ПРОГРАМУ
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Lab10T2 lab10task2 = new Lab10T2();
            await lab10task2.RunAsync();

            Console.WriteLine("Натисніть будь-яку клавішу для виходу...");
            Console.ReadKey();
        }
    }
}