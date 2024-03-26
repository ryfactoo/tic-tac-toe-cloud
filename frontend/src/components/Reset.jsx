import RandomGame from "./RandomGame";

function Reset({ gameState, onReset }) {
  return (
    <button onClick={onReset} className="reset-button">
      Restart
    </button>
  );
}

export default Reset;
