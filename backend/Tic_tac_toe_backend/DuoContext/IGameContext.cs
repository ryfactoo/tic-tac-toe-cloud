using System;
using Tic_tac_toe_backend.Models;

namespace Tic_tac_toe_backend.DuoContext
{
	public interface IGameContext
	{
		Game GetGame(Guid id);

		void AddGame(Game game);

        void RemoveGame(Guid id);

		void UpdateGame(Game game);

        IEnumerable<Game> GetGames();
	}
}

