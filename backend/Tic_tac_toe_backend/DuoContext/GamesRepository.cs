using System;
using Tic_tac_toe_backend.Models;

namespace Tic_tac_toe_backend.DuoContext
{
	public class GamesRepository : IGameContext
	{
        private Dictionary<Guid, Game> Games { get; set; } = new Dictionary<Guid, Game>();

        public Game GetGame(Guid id)
        {
            return Games.GetValueOrDefault(id);
        }

        public IEnumerable<Game> GetGames()
        {
            return Games.Values;
        }

        public void AddGame(Game game)
        {
            Games.Add(game.Id, game);

        }

        public void RemoveGame(Guid id)
        {
            Games.Remove(id);
        }

        public void UpdateGame(Game game)
        {
            Games[game.Id] = game;
        }
    }
}

