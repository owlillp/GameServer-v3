using AuthService.Contracts.Requests;
using Shared.Core.Abstractions;

namespace AuthService.Core.Features.Auth.Commands.Login;

public sealed record LoginCommand(LoginRequest Request) : ICommand;