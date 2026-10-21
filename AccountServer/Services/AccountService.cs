using AccountContracts;
using Grpc.Core;

namespace AccountServer.Services
{
    public class AccountGrpcService : AccountService.AccountServiceBase
    {
        private static readonly Dictionary<int, AccountResponse> Accounts = new()
        {
            [1] = new AccountResponse 
            {
                Id = 1,
                Username = "linecodesx",
                Email = "linecodesx@example.com",
                Role = "Admin"
            },
            [2] = new AccountResponse
            {
                Id = 2,
                Username = "student",
                Email = "student@example.com",
                Role = "User"
            }
        };

        public override Task<AccountResponse> GetAccount(AccountRequest request, ServerCallContext ctx)
        {
            if (request.Id <= 0)
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument,
                    "ID должен быть больше нуля."));
        }

        if (!Accounts.TryGetValue(request.Id, out var account))
        {
            throw new RpcException(
                new Status(StatusCode.NotFound,
                    "Аккаунт не найден."));
        }

        return Task.FromResult(account.Clone());
            
        }
    }
}