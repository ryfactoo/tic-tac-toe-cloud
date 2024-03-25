import { useState, useEffect } from "react";
import Board from "./Board";
import GameOver from "./GameOver";
import GameState from "./GameState";
import Reset from "./Reset";
import gameOverSoundAsset from "../sounds/game_over.wav";
import clickSoundAsset from "../sounds/click.wav";
import { API_URL } from "../App";

const gameOverSound = new Audio(gameOverSoundAsset);
gameOverSound.volume = 0.2;
const clickSound = new Audio(clickSoundAsset);
clickSound.volume = 0.5;

const winningCombinations = [
  //Rows
  { combo: [0, 1, 2], strikeClass: "strike-row-1" },
  { combo: [3, 4, 5], strikeClass: "strike-row-2" },
  { combo: [6, 7, 8], strikeClass: "strike-row-3" },

  //Columns
  { combo: [0, 3, 6], strikeClass: "strike-column-1" },
  { combo: [1, 4, 7], strikeClass: "strike-column-2" },
  { combo: [2, 5, 8], strikeClass: "strike-column-3" },

  //Diagonals
  { combo: [0, 4, 8], strikeClass: "strike-diagonal-1" },
  { combo: [2, 4, 6], strikeClass: "strike-diagonal-2" },
];

function checkWinner(tiles, setStrikeClass, setGameState) {
  for (const { combo, strikeClass } of winningCombinations) {
    const tileValue1 = tiles[combo[0]];
    const tileValue2 = tiles[combo[1]];
    const tileValue3 = tiles[combo[2]];

    if (
      tileValue1 !== null &&
      tileValue1 === tileValue2 &&
      tileValue1 === tileValue3
    ) {
      setStrikeClass(strikeClass);
      setGameState(GameState.finished);
      return;
    }
  }

  const areAllTilesFilledIn = tiles.every((tile) => tile !== null);
  if (areAllTilesFilledIn) {
    setGameState(GameState.finished);
  }
}

function TicTacToe({initialState}) {
  const [tiles, setTiles] = useState(Array(9).fill(null));
  const [playerTurn, setPlayerTurn] = useState(null);
  const [strikeClass, setStrikeClass] = useState();
  const [gameState, setGameState] = useState(null);
  const [player, setPlayer] = useState(null);

  useEffect(() =>{
    initGame(initialState)
  }, []);


  const handleTileClick = (index) => {
    if (gameState !== GameState.inProgress || playerTurn !== player) {
      return;
    }

    if (tiles[index] !== null) {
      return;
    }

    sendMove(index);
  };

  useEffect(() => {
    checkWinner(tiles, setStrikeClass, setGameState);
  }, [tiles]);

  // useEffect(() => {
  //   if (tiles.some((tile) => tile !== null)) {
  //     clickSound.play();
  //   }
  // }, [tiles]);

  useEffect(() => {
    if (playerTurn !== null){
      clickSound.play();
    }
  }, [playerTurn]);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await fetch(`${API_URL}/currentGame`, {
            method: 'GET',
            credentials: 'include'
        });
        const jsonData = await response.json();
        updateGame(jsonData);
      } catch (error) {
        console.error('Error fetching data:', error);
      }
    };

    fetchData();
    const interval = setInterval(fetchData, 1000);
    return () => clearInterval(interval);
  }, []);

  useEffect(() => {
    if (gameState === GameState.finished) {
      gameOverSound.play();
    }
  }, [gameState]);

  function updateGame(jsonData){
    setTiles(jsonData.board)
    setPlayerTurn(jsonData.currentPlayerSign)
    setGameState(jsonData.gameStatus)
  }

  function initGame(jsonData){
    const state = jsonData.state;
    const playerSign = jsonData.playerSign;
    
    setTiles(state.board)
    setPlayerTurn(state.currentPlayerSign)
    setGameState(state.gameStatus)
    setPlayer(playerSign);
  }

  const sendMove = async (index) => {
      try {
        const response = await fetch(`${API_URL}/currentGame/update/${index}`, {
            method: 'GET',
            credentials: 'include'
        });
        const jsonData = await response.json();
        updateGame(jsonData); // TODO test 
      } catch (error) {
        console.error('Error updating data:', error);
      }
  };

  return (
    <div>
      {gameState === GameState.pending ? (
                <div>
                  <p>Waiting for opponent...</p>
                </div>
            ) : (
                
      <div>
        <h1>Tic Tac Toe</h1>
        <Board
        playerTurn={playerTurn}
        tiles={tiles}
        onTileClick={handleTileClick}
        strikeClass={strikeClass}
        player={player}
      />
      <GameOver gameState={gameState} />
      {/* <Reset gameState={gameState} onReset={handleReset} /> */}
      </div>
            )}
    </div>
  );
}

export default TicTacToe;
