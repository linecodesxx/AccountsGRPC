using AccountServer.Data;
using AccountServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddSingleton<AccountStore>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<AccountGrpcService>();

app.Run();
