function Tile({ className, value, onClick, playerTurn, player }) {
  let hoverClass = '';
  if (value == null && playerTurn != null && playerTurn === player) {
    hoverClass = `${playerTurn.toLowerCase()}-hover`;
  }
  return (
    <div onClick={onClick} className={`tile ${className} ${hoverClass}`}>
      {value}
    </div>
  );
}

export default Tile;
