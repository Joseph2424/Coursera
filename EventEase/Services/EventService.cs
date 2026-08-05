using EventEase.Models;

namespace EventEase.Services
{
    public class EventService
    {
        private readonly List<Event> _events =
        [
            new Event
            {
                Id = 1,
                Name = "Tech Conference 2026",
                Date = new DateTime(2026, 9, 15),
                Location = "New York, NY",
                Description = "A conference about emerging technologies."
            },
            new Event
            {
                Id = 2,
                Name = "Music Festival",
                Date = new DateTime(2026, 7, 20),
                Location = "Austin, TX",
                Description = "Outdoor music festival with multiple artists."
            }
        ];

        public IEnumerable<Event> GetEvents() => _events;

        public Event? GetEventById(int id) =>
            _events.FirstOrDefault(e => e.Id == id);
    }
}
