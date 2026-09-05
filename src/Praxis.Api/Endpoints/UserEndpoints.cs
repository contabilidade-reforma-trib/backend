using Praxis.Identity.Application;
using Praxis.Identity.Domain;
using Praxis.Shared.Abstractions;

namespace Praxis.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/users").WithTags("Users");

        group.MapPost(string.Empty, CreateUser)
            .WithName("CreateUser")
            .WithSummary("Registers a user")
            .Produces<UserResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/{id:guid}", GetUser)
            .WithName("GetUser")
            .WithSummary("Reads one user")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet(string.Empty, ListUsers)
            .WithName("ListUsers")
            .WithSummary("Lists users, paged")
            .Produces<IReadOnlyCollection<UserResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreateUser(
        CreateUserRequest request,
        IUserRepository users,
        IClock clock,
        CancellationToken cancellationToken)
    {
        var created = User.Create(request.Name, request.Email, request.Phone, clock);

        if (created.Failed)
        {
            return Results.Problem(
                title: created.Error.Message,
                type: created.Error.Code,
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (await users.EmailIsTaken(created.Value.Email, cancellationToken))
        {
            return Results.Problem(
                title: "This email is already registered.",
                type: "user.email_taken",
                statusCode: StatusCodes.Status409Conflict);
        }

        await users.Add(created.Value, cancellationToken);
        await users.SaveChanges(cancellationToken);

        var response = UserResponse.From(created.Value);
        return Results.Created($"/api/users/{response.Id}", response);
    }

    private static async Task<IResult> GetUser(
        Guid id,
        IUserRepository users,
        CancellationToken cancellationToken)
    {
        var user = await users.GetById(id, cancellationToken);

        return user is null
            ? Results.Problem(title: "User not found.", type: "user.not_found", statusCode: StatusCodes.Status404NotFound)
            : Results.Ok(UserResponse.From(user));
    }

    private static async Task<IResult> ListUsers(
        IUserRepository users,
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 50)
    {
        var found = await users.List(Math.Max(skip, 0), Math.Clamp(take, 1, 200), cancellationToken);
        return Results.Ok(found.Select(UserResponse.From).ToList());
    }
}

public sealed record CreateUserRequest(string Name, string Email, string? Phone);

public sealed record UserResponse(Guid Id, string Name, string Email, string? Phone, DateTimeOffset CreatedAt)
{
    public static UserResponse From(User user) =>
        new(user.Id, user.Name, user.Email, user.Phone, user.CreatedAt);
}
