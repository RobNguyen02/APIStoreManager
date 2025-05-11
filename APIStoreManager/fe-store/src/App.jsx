import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Login from "./Login"; 
function App() {
    return (
        <Router>
            <Routes>
                <Route path="/login" element={<Login />} />
                {}
                <Route path="/" element={
                    <>
                        <h1>Vite + React</h1>
                        <a href="/login">Đi tới trang đăng nhập</a>
                    </>
                } />
            </Routes>
        </Router>
    );
}

export default App;
