import Input from "./Input";
import { API_URL } from "../App.js";
import { useState, useEffect} from "react";
import TicTacToe from "./TicTacToe.jsx";

function RandomGame(){
    const [currentNick, setCurrentNick] = useState('')
    const [initialState, setInitialState] = useState(null);

    useEffect(() => {
        if(currentNick !== ''){
            getNewRandomGame().then(result => {
                setInitialState(result);
            }).catch(error => {
                console.error('Error initializing game:', error);
            });
        }
    }, [currentNick]);
    
    function handleNickSubmit(nick) {
        setNick(nick)
            .then(result => {
                if (result === true) {
                    setCurrentNick(nick);
                }
            })
            .catch(error => {
                console.error('Error setting nick:', error);
            });
    }

    return (
        <div>
            {currentNick === '' ? (
                <div>
                    <Input placeholder="Enter nick" onSubmit={handleNickSubmit} />
                </div>
            ) : (
                initialState ? (
                    <TicTacToe initialState={initialState} />
                ) : (
                    <p>Loading...</p>
                )
            )}
        </div>
    );
}

async function getNewRandomGame(){
    const response = await fetch(`${API_URL}/newGame/random`, {
            method: 'GET',
            credentials: 'include'
        })
    const game = await response.json();
    return game;
}

async function setNick(nick) {
    const response = await fetch(`${API_URL}/nick/${nick}`, {
            method: 'GET',
            credentials: 'include'
        });
    return response.ok;
}

export default RandomGame;