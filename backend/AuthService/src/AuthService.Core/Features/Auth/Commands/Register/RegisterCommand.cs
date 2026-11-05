using AuthService.Contracts.Requests;
using Shared.Core.Abstractions;

namespace AuthService.Core.Features.Auth.Commands.Register;

public sealed record RegisterCommand(RegisterRequest Request) : ICommand;