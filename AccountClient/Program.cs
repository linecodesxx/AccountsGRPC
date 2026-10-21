using AccountContracts;
using Grpc.Core;
using Grpc.Net.Client;

using var channel = GrpcChannel.ForAddress("http://localhost:5057");

var client = new AccountService.AccountServiceClient(channel);

while (true)
{
    Console.WriteLine("""

        1 - Показать все аккаунты
        2 - Найти аккаунт по ID
        3 - Создать аккаунт
        4 - Обновить аккаунт
        5 - Удалить аккаунт
        exit - Выход
        """);
    Console.Write("Выберите действие: ");

    var input = Console.ReadLine();

    if (input is null ||
        input.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    switch (input.Trim())
    {
        case "1":
            await GetAllAccounts(client);
            break;
        case "2":
            await GetAccount(client);
            break;
        case "3":
            await CreateAccount(client);
            break;
        case "4":
            await UpdateAccount(client);
            break;
        case "5":
            await DeleteAccount(client);
            break;
        default:
            Console.WriteLine("Неизвестное действие.");
            break;
    }
}

static DateTime CreateDeadline() => DateTime.UtcNow.AddSeconds(5);

static int? ReadId()
{
    Console.Write("Введите ID аккаунта: ");
    var input = Console.ReadLine();

    if (!int.TryParse(input, out var id) || id <= 0)
    {
        Console.WriteLine("Введите положительное целое число.");
        return null;
    }

    return id;
}

static string? ReadValue(string label)
{
    Console.Write($"{label}: ");
    var value = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(value))
    {
        Console.WriteLine("Значение не может быть пустым.");
        return null;
    }

    return value.Trim();
}

static void PrintAccount(AccountResponse account)
{
    Console.WriteLine($"""

        ID:    {account.Id}
        Имя:   {account.Username}
        Email: {account.Email}
        Роль:  {account.Role}
        """);
}

static void PrintError(RpcException ex)
{
    Console.WriteLine($"Ошибка ({ex.StatusCode}): {ex.Status.Detail}");
}

static async Task GetAllAccounts(AccountService.AccountServiceClient client)
{
    try
    {
        var result = await client.GetAllAsync(
            new Empty(),
            deadline: CreateDeadline());

        if (result.Accounts.Count == 0)
        {
            Console.WriteLine("Аккаунтов нет.");
            return;
        }

        foreach (var account in result.Accounts)
        {
            PrintAccount(account);
        }
    }
    catch (RpcException ex)
    {
        PrintError(ex);
    }
}

static async Task GetAccount(AccountService.AccountServiceClient client)
{
    var id = ReadId();
    if (id is null)
    {
        return;
    }

    try
    {
        var account = await client.GetAccountAsync(
            new AccountRequest { Id = id.Value },
            deadline: CreateDeadline());

        PrintAccount(account);
    }
    catch (RpcException ex)
    {
        PrintError(ex);
    }
}

static async Task CreateAccount(AccountService.AccountServiceClient client)
{
    var username = ReadValue("Имя пользователя");
    if (username is null)
    {
        return;
    }

    var email = ReadValue("Email");
    if (email is null)
    {
        return;
    }

    var password = ReadValue("Пароль");
    if (password is null)
    {
        return;
    }

    var role = ReadValue("Роль");
    if (role is null)
    {
        return;
    }

    try
    {
        var account = await client.CreateAccountAsync(
            new CreateAccountRequest
            {
                Username = username,
                Email = email,
                Password = password,
                Role = role
            },
            deadline: CreateDeadline());

        Console.WriteLine("Аккаунт создан.");
        PrintAccount(account);
    }
    catch (RpcException ex)
    {
        PrintError(ex);
    }
}

static async Task UpdateAccount(AccountService.AccountServiceClient client)
{
    var id = ReadId();
    if (id is null)
    {
        return;
    }

    var username = ReadValue("Имя пользователя");
    if (username is null)
    {
        return;
    }

    var email = ReadValue("Email");
    if (email is null)
    {
        return;
    }

    var password = ReadValue("Пароль");
    if (password is null)
    {
        return;
    }

    var role = ReadValue("Роль");
    if (role is null)
    {
        return;
    }

    try
    {
        var account = await client.UpdateAccountAsync(
            new UpdateAccountRequest
            {
                Id = id.Value,
                Username = username,
                Email = email,
                Password = password,
                Role = role
            },
            deadline: CreateDeadline());

        Console.WriteLine("Аккаунт обновлён.");
        PrintAccount(account);
    }
    catch (RpcException ex)
    {
        PrintError(ex);
    }
}

static async Task DeleteAccount(AccountService.AccountServiceClient client)
{
    var id = ReadId();
    if (id is null)
    {
        return;
    }

    try
    {
        await client.DeleteAccountAsync(
            new AccountRequest { Id = id.Value },
            deadline: CreateDeadline());

        Console.WriteLine($"Аккаунт {id} удалён.");
    }
    catch (RpcException ex)
    {
        PrintError(ex);
    }
}
