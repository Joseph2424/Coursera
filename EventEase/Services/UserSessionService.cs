using Microsoft.JSInterop;
using System.Text.Json;

namespace EventEase.Services
{
    public class UserSessionService(IJSRuntime js)
    {
        private readonly IJSRuntime _js = js;

        public required string SessionId { get; set; }
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public List<SessionEvent> Events { get; set; } = [];

        public List<int> RegisteredEventIds { get; set; } = [];
        public List<int> VisitedEventIds { get; set; } = [];
        public List<int> AttendedEventIds { get; set; } = [];

        public async Task TrackEvent(string eventName, string? details = null)
        {
            Events.Add(new SessionEvent
            {
                EventName = eventName,
                Details = details,
                Timestamp = DateTime.UtcNow
            });

            await SaveToLocalStorage();
        }

        public async Task LoadFromLocalStorageAsync()
        {
            var json = await _js.InvokeAsync<string>("sessionTracker.loadSession", SessionId);

            if (!string.IsNullOrWhiteSpace(json))
            {
                var data = JsonSerializer.Deserialize<UserSessionData>(json);
                if (data != null)
                {
                    StartTime = data.StartTime;
                    Events = data.Events ?? [];
                    RegisteredEventIds = data.RegisteredEventIds ?? [];
                    VisitedEventIds = data.VisitedEventIds ?? [];
                    AttendedEventIds = data.AttendedEventIds ?? [];
                }
            }
        }

        private async Task SaveToLocalStorage()
        {
            var data = new UserSessionData
            {
                SessionId = SessionId,
                StartTime = StartTime,
                Events = Events,
                RegisteredEventIds = RegisteredEventIds,
                VisitedEventIds = VisitedEventIds,
                AttendedEventIds = AttendedEventIds

            };

            var json = JsonSerializer.Serialize(data);
            await _js.InvokeVoidAsync("sessionTracker.saveSession", SessionId, json);
        }

        public async Task AddEventToSession(int eventId)
        {
            if (!RegisteredEventIds.Contains(eventId))
            {
                RegisteredEventIds.Add(eventId);
                await SaveToLocalStorage();
            }
        }

        public async Task MarkEventVisited(int eventId)
        {
            if (!VisitedEventIds.Contains(eventId))
            {
                VisitedEventIds.Add(eventId);
                await SaveToLocalStorage();
            }
        }

        public async Task MarkEventAttended(int eventId)
        {
            if (!AttendedEventIds.Contains(eventId))
            {
                AttendedEventIds.Add(eventId);
                await SaveToLocalStorage();
            }
        }

        public async Task RemoveEventRegistration(int eventId)
        {
            if (RegisteredEventIds.Contains(eventId))
            {
                RegisteredEventIds.Remove(eventId);
                await SaveToLocalStorage();
            }

            if (AttendedEventIds.Contains(eventId))
            {
                AttendedEventIds.Remove(eventId);
                await SaveToLocalStorage();
            }
        }
    }

    public class UserSessionData
    {
        public required string SessionId { get; set; }
        public DateTime StartTime { get; set; }
        public List<SessionEvent>? Events { get; set; }
        public List<int> RegisteredEventIds { get; set; } = [];
        public List<int> VisitedEventIds { get; set; } = [];
        public List<int> AttendedEventIds { get; set; } = [];
    }



    public class SessionEvent
    {
        public string? EventName { get; set; }
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
