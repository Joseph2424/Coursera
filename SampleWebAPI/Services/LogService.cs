namespace SampleWebAPI.Services
{
    public class LogService : ILogService
    {
        private readonly int _serviceId;

        public LogService()
        {
            _serviceId = new Random().Next(1, 1000);
        }

        public void Log(string message)
        {
            Console.WriteLine($"[{DateTime.Now}] [{_serviceId}] {message}");
        }
    }
}