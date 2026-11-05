using AuthService.Contracts.Requests;
using AuthService.Contracts.Responses;
using AuthService.Core.Features.Auth.Commands.Login;
using AuthService.Core.Features.Auth.Commands.Register;
using Microsoft.AspNetCore.Mvc;
using Shared.Core.Abstractions;
using Shared.Framework.Endpoints;

namespace AuthService.Web.Endpoints.Auth;

public sealed class AuthEndpoints : IEndpointGroup
{
    private const string RESOURCE = "/auth";

    public RouteGroupBuilder MapGroup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup(RESOURCE)
            .WithTags("Auth")
            .RequireAuthorization();

        group.MapPost("/login", Login)
            .WithName("Auth.Login")
            .WithSummary("Вход по email и паролю")
            .AllowAnonymous();

        group.MapPost("/register", Register)
            .WithName("Auth.Register")
            .WithSummary("Регистрация аккаунта")
            .AllowAnonymous();

        return group;
    }

    private static async Task<EndpointResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        [FromServices] ICommandHandler<LoginResponse, LoginCommand> handler,
        CancellationToken ct) =>
        await handler.Handle(new LoginCommand(request), ct);

    private static async Task<EndpointResult<RegisterResponse>> Register(
        [FromBody] RegisterRequest request,
        [FromServices] ICommandHandler<RegisterResponse, RegisterCommand> handler,
        CancellationToken ct) =>
        await handler.Handle(new RegisterCommand(request), ct);
}
