using System;
using Tic_tac_toe_backend.Models;

namespace Tic_tac_toe_backend.Models
{
	public class Game
	{
		public static readonly int boardSize = 9;
		public static readonly string[] playerSigns = { "X", "O" };
        private readonly List<List<int>> winningCombinations = new List<List<int>>
		{
			new List<int> {0, 1, 2},
			new List<int> {3, 4, 5},
			new List<int> {6, 7, 8},
			new List<int> {0, 3, 6},
			new List<int> {1, 4, 7},
			new List<int> {2, 5, 8},
			new List<int> {0, 4, 8},
			new List<int> {2, 4, 6}
		};
        public Guid Id { get; } = new Guid();
		public Dictionary<string, (string, int)> Players { get; set; } = new Dictionary<string, (string, int)>();
		public GameState State { get; set; } = new GameState();
		public GameStatusType Status { get { return State.GameStatus; } set { State.GameStatus = value; } }



		public bool CheckWin()
		{
            var isWin = winningCombinations.Any(
                            combination => combination
                                .All(index => State.CurrentPlayerSign
                                        .Equals(State.Board[index]
                            )));

            if (isWin)
            {
                State.GameStatus = GameStatusType.Finished;
            }

            return isWin;

        }

		public bool CheckEnd()
		{
			for(var i = 0; i < boardSize; i++)
			{
				if (State.Board[i] == null)
				{
					return false;
				}
			}

            Status = GameStatusType.Finished;
            return true;


        }

    }
}
