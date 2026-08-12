using UserManagementAPI.Models;

namespace UserManagementAPI.Services
{
    public class UserRepository
    {
        private readonly Dictionary<int, User> _users = new();
        private int _nextId = 1;

        public IEnumerable<User> GetAll() => _users.Values;

        public User Get(int id) =>
            _users.TryGetValue(id, out var user) ? user : null;

        public User Create(User user)
        {
            user.Id = _nextId++;
            _users[user.Id] = user;
            return user;
        }

        public bool Update(int id, User updated)
        {
            if (!_users.ContainsKey(id)) return false;
            updated.Id = id;
            _users[id] = updated;
            return true;
        }

        public bool Delete(int id) =>
            _users.Remove(id);

        public IEnumerable<User> Search(string term)
        {
            term = term.ToLower();
            return _users.Values.Where(u =>
                u.Name.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term));
        }
    }
}
