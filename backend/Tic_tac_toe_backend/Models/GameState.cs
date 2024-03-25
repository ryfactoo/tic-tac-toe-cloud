using System;
namespace Tic_tac_toe_backend.Models
{
	public class GameState
	{

		public string[] Board { get; set; }
		public string CurrentPlayerSign { get {return Game.playerSigns[CurrentPlayerId]; } set {; } }
        public int CurrentPlayerId { get; set; }
        public GameStatusType GameStatus { get; set; }


        public GameState()
        {
            Board = new string[Game.boardSize];
            CurrentPlayerId = 0;
            GameStatus = GameStatusType.Pending;
        }

        
    }

    public enum GameStatusType
    {
        Pending = 0,
        InProgress = 1,
        Finished = 2
    }


}
