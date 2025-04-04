namespace SystemLogMonitor;

// --------------------------------------------------------
// Author Group: Java Beans
// Comment: Please fix your problem.
// Created: April 4, 2025
// --------------------------------------------------------

public class Program
{
    static void Main()
    {
        var logMonitor = new SystemLogMonitor();
        logMonitor.MenuLoop();
    }

   
    // LogEntry.cs
    public class LogEntry
    {
        public DateTimeOffset Timestamp { get; set; }

        public EventType EventType { get; set; }

        public string Description { get; set; }

        public override string ToString()
        {
            return $"Timestamp: {Timestamp} | EventType: {EventType} | Description: {Description}";
        }
    }
    
    // SystemLogMonitor.cs
    public class SystemLogMonitor : ISystemLogMonitor
    {
        readonly Stack<LogEntry> m_entries = new();
        
        public void MenuLoop()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("--- System Log Monitor ---\n");
            Console.ResetColor();
            
            while (true)
            {
                string consoleFormat =
                    $"1. Add a Log Entry\n2. View All Logs (Most Recent First)\n3. Filter Logs by Event Type\n4. Undo Last Log Entry\n5. Exit";
                Console.WriteLine(consoleFormat);
                
                Console.Write("Enter your choice: ");
                string? logChoice = Console.ReadLine();
                int type = Int32.Parse(logChoice);
                
                switch (type)
                {
                    case 1: 
                        AddLogEntry(logChoice);
                        
                        break;
                    case 2:
                        ViewLogs();
                        break;
                    case 3:
                        Console.WriteLine("Enter event type to filter: ");
                        string? eventType = Console.ReadLine();
                        
                        var convertedType = LogHelpers.ConvertType(eventType);
                        FilterLogs(convertedType);
                        break;
                    case 4:
                        UndoEntry();
                        break;
                    case 5:
                        Console.ForegroundColor = ConsoleColor.Green;
                        LogHelpers.LogExistSuccessfully();
                        Console.ResetColor();
                        break;
                }
                
                if (type == 5) break;
                
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Creates a newly log entry
        /// </summary>
        private void AddLogEntry(string logChoice)
        {
            // User choice
            int type = Int32.Parse(logChoice);
            var entry = LogHelpers.Prompt(type);
            m_entries.Push(entry);
        }
        
        
        /// <summary>
        /// View All Logs (Most Recent First): Display all logs, most recent at the top (LIFO order).
        /// </summary>
        private void ViewLogs()
        {
            ViewAllLogs();
        }
        
        /// <summary>
        /// Filter Logs by Event Type: Show only the logs that match a given event type (e.g., "ERROR").
        /// </summary>
        private void FilterLogs(EventType eventType)
        {
            FilterLogsByType(eventType.ToString());
        }
        
        /// <summary>
        /// Undo Last Log Entry: Remove the most recently added log from the system.
        /// </summary>
        private void UndoEntry()
        {
            if (m_entries.Count() == 0)
            {
                Console.WriteLine("\nNo logs to undo.");
                return;
            }
            
            var existingEntry = m_entries.Peek();
            
            m_entries.Pop();
            
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Entry removed!");
            Console.ResetColor();
            
            LogHelpers.LogEntry(existingEntry);
        }

        private void ViewAllLogs()
        {
            if (m_entries.Count == 0)
            {
                LogHelpers.LogEntryNotFound();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("--- All Logs (Most Recent First) ---");
            Console.ResetColor();
            
            foreach (var entry in m_entries.Reverse())
            {
                LogHelpers.LogEntry(entry);
            }
        }

        private void FilterLogsByType(string eventType)
        {
            foreach (var entry in m_entries)
            {
                var type = entry.EventType.ToString();

                if (!string.IsNullOrWhiteSpace(eventType))
                {
                    var result = String.Compare(type, eventType, StringComparison.Ordinal);
                    
                    if (result != 0)
                    {
                        LogHelpers.LogEntryNotFound();
                        return;
                    }
                    
                    LogHelpers.LogEntry(entry);
                }
            }
        }
    }
    
    // LogHelpers.cs
    public static class LogHelpers
    {
        public static EventType ConvertType(string eventType)
        {
            eventType = eventType.Trim();
            
            switch (eventType)
            {
                case "login":
                case "LOGIN":
                    return EventType.LOGIN;
                case "logout":
                case  "LOGOUT":
                    return EventType.LOGOUT;
                case "access":
                case "ACCESS":
                    return EventType.ACCESS;
                case "error":
                case "ERROR":
                    return EventType.ERROR;
            }

            return EventType.OOPS;
        }
        public static LogEntry Prompt(int logChoice, string logType = "")
        {
            switch (logChoice)
            {
                case 1: 
                    logType = EventConstants.LogTypeLogin;
                    break;
                case 2: 
                    logType = EventConstants.LogTypeAccess;
                    break;
                case 3: 
                    logType = EventConstants.LogTypeAccess;
                    break;
                case 4:
                    logType = EventConstants.LogTypeUndo;
                    break;
                case 5:
                    logType = EventConstants.LogTypeExit;
                    break;
            }
            
            switch (logType)
            {
                case "LOGIN":
                    var loginEntry = LogEntryFactory.CreateLogEntry(DateTimeOffset.Now, EventType.LOGIN, EventConstants.EventLogin);
                    LogEntry(loginEntry);
                    LogEntryAddedSuccessful();
                    return loginEntry;
                case  "LOGOUT":
                {
                    var logoutEntry = LogEntryFactory.CreateLogEntry(DateTimeOffset.Now, EventType.LOGOUT, EventConstants.EventLogout);
                    LogEntry(logoutEntry);
                    Console.WriteLine(EventConstants.EventLogout);
                    return logoutEntry;
                    
                }
                case "ACCESS":
                    var accessEntry = LogEntryFactory.CreateLogEntry(DateTimeOffset.Now, EventType.ACCESS, EventConstants.EventAccess);
                    LogSuccessful(accessEntry.Timestamp, EventConstants.EventAccess, EventConstants.LogTypeLogin);
                    return accessEntry;
                    
                case "ERROR":
                    var errorEntry = LogEntryFactory.CreateLogEntry(DateTimeOffset.Now, EventType.ERROR, EventConstants.EventError);
                    LogEntry(errorEntry);
                    Console.WriteLine(EventConstants.EventError);
                    return errorEntry;
                case "EXIT":
                    Console.WriteLine();
                    return null;
            }
            return null;
        }
        
        public static void LogEntry(LogEntry entry)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            string formatEntry = $"\n{entry.Timestamp.ToString("yyyy-MM-dd HH:mm")}\n" +
                                 $"{entry.EventType} \n" +
                                 $"{entry.Description}";
            Console.WriteLine(formatEntry);
            Console.ResetColor();
        }

        public static void LogSuccessful(DateTimeOffset timeOffset, string eventAccess, string eventType = "")
        {
            if (eventType.Length != 0)
            {
                Console.WriteLine($"[{timeOffset.ToString()}] {eventType}: {eventAccess}");
                return;
            }
            
            Console.WriteLine($"[{timeOffset.ToString()}] {eventAccess}");
        }

        public static void LogEntryAddedSuccessful() => Console.WriteLine("\nLog entry added.");
        public static void LogExistSuccessfully() => Console.Write(EventConstants.EventExit);

        public static void LogEntryNotFound()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nNo log entries to display.");
            Console.ResetColor();
        }
        
    }

    // LogEntryFactory.cs
    public static class LogEntryFactory
    {
        public static LogEntry CreateLogEntry(DateTimeOffset timeStamp, EventType logType, string description)
        {
            return new LogEntry() { Timestamp = timeStamp, EventType = logType, Description = description };
        }
    }
    
    // EventConstants.cs
    public static class EventConstants
    {
        public static string EventLogin = "Admin user logged in successfully";
        public const string EventLogout = "Admin user logged out successfully";
        public const string EventAccess = "Admin user access successfully";
        public const string EventError = "Admin user access successfully"; // TODO
        public const string EventExit = "Exiting System Log Monitor. Goodbye!";

        public static string LogTypeLogin = "LOGIN";
        public static string LogTypeLogout = "LOGOUT";
        public static string LogTypeAccess = "ACCESS";
        public static string LogTypeError = "ERROR";
        public static string LogTypeUndo = "UNDO";
        public static string LogTypeExit = "EXIT";
    }
    
    // EventType.cs
    public enum EventType
    {
        LOGIN,
        ERROR,
        LOGOUT,
        ACCESS,
        OOPS
    }

}

public interface ISystemLogMonitor
{
    /// <summary>
    /// Initializes a menu loop.
    /// </summary>
    void MenuLoop();
}
