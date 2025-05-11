import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter, Routes, Route, Navigate, useLocation } from "react-router-dom";
import Login from "./pages/Login";
import Register from "./pages/Register";
import Dashboard from "./pages/Dashboard";
import CreateShop from "./pages/CreateShop";
import CreateProduct from "./pages/CreateProduct";
import UpdateProduct from "./pages/UpdateProduct";

import "./index.css";

const isLoggedIn = () => !!localStorage.getItem("token");

const PrivateRoute = ({ children }) => {
    const location = useLocation();
    return isLoggedIn() ? children : <Navigate to="/login" state={{ from: location }} replace />;
};

ReactDOM.createRoot(document.getElementById("root")).render(
    <BrowserRouter>
        <Routes>
            {}
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />

            {}
            <Route
                path="/dashboard"
                element={
                    <PrivateRoute>
                        <Dashboard />
                    </PrivateRoute>
                }
            />

            <Route
                path="/shop/create"
                element={
                    <PrivateRoute>
                        <CreateShop />
                    </PrivateRoute>
                }
            />

            <Route
                path="/shop/:shopId/product/create"
                element={
                    <PrivateRoute>
                        <CreateProduct />
                    </PrivateRoute>
                }
            />
            <Route
                path="/product/:productId/edit"
                element={<UpdateProduct />}
            />

            {}
            <Route path="/" element={<Navigate to="/dashboard" replace />} />
            <Route path="/product/create" element={<Navigate to="/dashboard" replace />} />
        </Routes>
    </BrowserRouter>
);