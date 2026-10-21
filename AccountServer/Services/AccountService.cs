using AccountContracts;
using AccountServer.Data;
using Grpc.Core;

namespace AccountServer.Services
{
    public class AccountGrpcService : AccountService.AccountServiceBase
    {
        private readonly AccountStore _store;

        public AccountGrpcService(AccountStore store)
        {
            _store = store;
        }

        public override Task<AccountResponse> GetAccount(AccountRequest request, ServerCallContext ctx)
        {
            if (request.Id <= 0)
            {
                throw new RpcException(
                    new Status(StatusCode.InvalidArgument,
                        "ID должен быть больше нуля."));
            }

            var account = _store.Get(request.Id);

            if (account is null)
            {
                throw new RpcException(
                    new Status(StatusCode.NotFound,
                        "Аккаунт не найден."));
            }

            return Task.FromResult(account);
        }

        public override Task<AccountList> GetAll(Empty request, ServerCallContext context)
        {
            var accountList = new AccountList();
            accountList.Accounts.AddRange(_store.GetAll());

            return Task.FromResult(accountList);
        }

        public override Task<AccountResponse> CreateAccount(CreateAccountRequest request, ServerCallContext context)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Email))
            {
                throw new RpcException(
                    new Status(StatusCode.InvalidArgument,
                        "Имя пользователя и email обязательны."));
            }

            var account = _store.Create(
                request.Username,
                request.Email,
                request.Role);

            return Task.FromResult(account);
        }

        public override Task<AccountResponse> UpdateAccount(UpdateAccountRequest request, ServerCallContext context)
        {
            if (request.Id <= 0)
            {
                throw new RpcException(
                    new Status(StatusCode.InvalidArgument,
                        "ID должен быть больше нуля."));
            }

            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Email))
            {
                throw new RpcException(
                    new Status(StatusCode.InvalidArgument,
                        "Имя пользователя и email обязательны."));
            }

            var account = _store.Update(
                request.Id,
                request.Username,
                request.Email,
                request.Role);

            if (account is null)
            {
                throw new RpcException(
                    new Status(StatusCode.NotFound,
                        "Аккаунт не найден."));
            }

            return Task.FromResult(account);
        }

        public override Task<Empty> DeleteAccount(AccountRequest request, ServerCallContext context)
        {
            if (request.Id <= 0)
            {
                throw new RpcException(
                    new Status(StatusCode.InvalidArgument,
                        "ID должен быть больше нуля."));
            }

            if (!_store.Delete(request.Id))
            {
                throw new RpcException(
                    new Status(StatusCode.NotFound,
                        "Аккаунт не найден."));
            }

            return Task.FromResult(new Empty());
        }
    }
}
