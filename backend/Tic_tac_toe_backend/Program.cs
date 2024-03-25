using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Http;
using Tic_tac_toe_backend.Models;
using Tic_tac_toe_backend.DuoContext;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = false;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000")
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
});

builder.Services.AddSingleton<IGameContext, GamesRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowSpecificOrigin");

app.UseSession();


app.UseHttpsRedirection();


app.MapGet("/nick/{nick}", (string nick, HttpContext httpContext) =>
{
    if(nick == null || nick.Trim().Length == 0)
    {
        return Results.BadRequest();
    }

    httpContext.Session.SetString("nick", nick);
    return Results.Ok();
});


app.MapGet("/newGame/random", (HttpContext httpContext, IGameContext gameContext) =>
{
    var sessionId = httpContext.Request.Cookies[".AspNetCore.Session"];
    var nick = httpContext.Session.GetString("nick");

    if (sessionId == null || nick == null)
    {
        return Results.BadRequest();
    }


    var game = AsingToGame(sessionId, nick, gameContext);
    var (_, playerId) = game.Players[sessionId];

    return Results.Ok(new { state = game.State, playerSign = Game.playerSigns[playerId] });
})
.WithOpenApi();


app.MapGet("/currentGame", (HttpContext httpContext, IGameContext gameContext) =>
{
    var sessionId = httpContext.Request.Cookies[".AspNetCore.Session"];

    if (sessionId == null)
    {
        return Results.BadRequest();
    }


    var currentGame = GetCurrentGame(sessionId, gameContext);

    if (currentGame == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(currentGame.State);
})
.WithOpenApi();


app.MapGet("/currentGame/update/{cellIndex}", (int cellIndex, HttpContext httpContext, IGameContext gameContext) =>
{
    var sessionId = httpContext.Request.Cookies[".AspNetCore.Session"];

    if (sessionId == null)
    {
        return Results.BadRequest();
    }

    var state = UpdateGame(cellIndex, sessionId, gameContext);

    return state != null ? Results.Ok(state) : Results.BadRequest();
})
.WithOpenApi();


app.Run();


Game GetPendingGame(IGameContext gameContext)
{
    var game = gameContext.GetGames()
        .Where(game => game.Status == GameStatusType.Pending )
        .FirstOrDefault();

    return game;
}


Game GetCurrentGame(string sessionId, IGameContext gameContext)
{
    var game = gameContext.GetGames()
        .Where(game => game.Players.ContainsKey(sessionId))
        .LastOrDefault();

    return game;
}


Game AsingToGame(string sessionId, string nick, IGameContext gameContext)
{
    var pendingGame = GetPendingGame(gameContext);

    if (pendingGame == null)
    {
        pendingGame = new Game();
        pendingGame.Players.Add(sessionId, (nick, 0));
        gameContext.AddGame(pendingGame);
    }
    else
    {
        pendingGame.Players.Add(sessionId, (nick, 1));
        pendingGame.Status = GameStatusType.InProgress;
        gameContext.UpdateGame(pendingGame);
    }

    return pendingGame;
}


GameState UpdateGame(int cellIndex, string sessionId, IGameContext gameContext)
{
    var currentGame = GetCurrentGame(sessionId, gameContext);
    var currentState = currentGame.State;
    var (_, playerId) = currentGame.Players[sessionId];

    if (playerId == currentState.CurrentPlayerId
        && cellIndex >= 0 && cellIndex < Game.boardSize
        && currentState.Board[cellIndex] == null
        && currentState.GameStatus == GameStatusType.InProgress)
    {
        currentState.Board[cellIndex] = Game.playerSigns[playerId];
        currentGame.CheckWin();
        currentGame.CheckEnd();
        currentState.CurrentPlayerId = (currentState.CurrentPlayerId + 1) % 2;
        gameContext.UpdateGame(currentGame);
        return currentState;
    }

    return null;
}