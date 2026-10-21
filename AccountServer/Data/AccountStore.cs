using System.Text.Json;
using AccountContracts;

namespace AccountServer.Data
{
    public class AccountStore
    {
        private sealed class AccountRecord
        {
            public int Id { get; set; }
            public string Username { get; set; } = "";
            public string Email { get; set; } = "";
            public string Role { get; set; } = "";
        }

        private readonly string _filePath;
        private readonly object _sync = new();
        private readonly Dictionary<int, AccountRecord> _accounts = new();

        public AccountStore(IHostEnvironment environment)
        {
            _filePath = Path.Combine(environment.ContentRootPath, "accounts.json");
            Load();
        }

        public List<AccountResponse> GetAll()
        {
            lock (_sync)
            {
                return _accounts.Values
                    .OrderBy(account => account.Id)
                    .Select(ToResponse)
                    .ToList();
            }
        }

        public AccountResponse? Get(int id)
        {
            lock (_sync)
            {
                return _accounts.TryGetValue(id, out var account)
                    ? ToResponse(account)
                    : null;
            }
        }

        public AccountResponse Create(string username, string email, string role)
        {
            lock (_sync)
            {
                var id = _accounts.Count == 0 ? 1 : _accounts.Keys.Max() + 1;

                var account = new AccountRecord
                {
                    Id = id,
                    Username = username,
                    Email = email,
                    Role = role
                };

                _accounts[id] = account;
                Save();

                return ToResponse(account);
            }
        }

        public AccountResponse? Update(int id, string username, string email, string role)
        {
            lock (_sync)
            {
                if (!_accounts.TryGetValue(id, out var account))
                {
                    return null;
                }

                account.Username = username;
                account.Email = email;
                account.Role = role;
                Save();

                return ToResponse(account);
            }
        }

        public bool Delete(int id)
        {
            lock (_sync)
            {
                if (!_accounts.Remove(id))
                {
                    return false;
                }

                Save();
                return true;
            }
        }

        private void Load()
        {
            if (!File.Exists(_filePath))
            {
                Seed();
                Save();
                return;
            }

            try
            {
                var json = File.ReadAllText(_filePath);
                var records = JsonSerializer.Deserialize<List<AccountRecord>>(json) ?? [];

                foreach (var record in records)
                {
                    _accounts[record.Id] = record;
                }
            }
            catch (Exception ex) when (ex is JsonException or IOException)
            {
                throw new InvalidOperationException(
                    $"Не удалось прочитать файл данных '{_filePath}'.", ex);
            }
        }

        private void Save()
        {
            var json = JsonSerializer.Serialize(
                _accounts.Values.OrderBy(account => account.Id),
                new JsonSerializerOptions { WriteIndented = true });

            var tempPath = _filePath + ".tmp";
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, _filePath, overwrite: true);
        }

        private void Seed()
        {
            _accounts[1] = new AccountRecord
            {
                Id = 1,
                Username = "linecodesx",
                Email = "linecodesx@example.com",
                Role = "Admin"
            };

            _accounts[2] = new AccountRecord
            {
                Id = 2,
                Username = "student",
                Email = "student@example.com",
                Role = "User"
            };
        }

        private static AccountResponse ToResponse(AccountRecord account) => new()
        {
            Id = account.Id,
            Username = account.Username,
            Email = account.Email,
            Role = account.Role
        };
    }
}
