using AccountContracts;
using Grpc.Core;
using Grpc.Net.Client;

using var channel = GrpcChannel.ForAddress("http://localhost:5000");

var client = new AccountService.AccountServiceClient(channel);

while (true)
{
    Console.Write("\nВведите ID аккаунта или exit: ");
    var input = Console.ReadLine();

    if (input is null ||
        input.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    if (!int.TryParse(input, out var id) || id <= 0)
    {
        Console.WriteLine("Введите положительное целое число.");
        continue;
    }

    try
    {
        var account = await client.GetAccountAsync(
            new AccountRequest { Id = id },
            deadline: DateTime.UtcNow.AddSeconds(5));

        Console.WriteLine($"""
            
            ID:    {account.Id}
            Имя:   {account.Username}
            Email: {account.Email}
            Роль:  {account.Role}
            """);
    }
    catch (RpcException ex)
    {
        Console.WriteLine(
            $"Ошибка ({ex.StatusCode}): {ex.Status.Detail}");
    }
}