import "./App.css";
import RandomGame from "./components/RandomGame";

const API_DOMAIN = process.env.REACT_APP_API_DOMAIN;
const API_PORT = process.env.REACT_APP_API_PORT;
export const API_URL = `${API_DOMAIN}:${API_PORT}`;

function App() {

  return (
    <div>
      <RandomGame/>
    </div>
  );

}


export default App;
