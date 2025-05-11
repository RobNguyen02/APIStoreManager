import React, { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import axios from "axios";
import { jwtDecode } from "jwt-decode";

function CreateProduct() {
    const navigate = useNavigate();
    const { shopId } = useParams();
    const [form, setForm] = useState({
        name: "",
        price: "",
        description: "",
        shopId: shopId || ""
    });
    const [shops, setShops] = useState([]);
    const [error, setError] = useState("");
    const [isLoading, setIsLoading] = useState(false);
    const [isShopLoading, setIsShopLoading] = useState(false);

    const token = localStorage.getItem("accessToken");
    const decoded = token ? jwtDecode(token) : null;
    const userId = decoded?.nameid || decoded?.sub;

    useEffect(() => {
        if (!token) {
            navigate("/login");
            return;
        }

        const fetchShops = async () => {
            setIsShopLoading(true);
            try {
                const response = await axios.get("http://localhost:5084/api/shop/MyShopsList", {
                    headers: { Authorization: `Bearer ${token}` }
                });
                setShops(response.data);

                if (shopId) {
                    const shop = response.data.find(shop => shop.id == shopId);
                    if (!shop) {
                        setError("Bạn không có quyền thêm sản phẩm vào shop này");

                    } else {
                        setForm(prev => ({ ...prev, shopId }));
                    }
                }
            } catch (err) {
                setError(err.response?.data?.Message || "Không thể tải danh sách shop");
            } finally {
                setIsShopLoading(false);
            }
        };

        fetchShops();
    }, [token, shopId, navigate]);

    const handleChange = (e) => {
        setForm({
            ...form,
            [e.target.name]: e.target.value
        });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setIsLoading(true);
        setError("");

        try {
            const price = parseFloat(form.price);
            if (isNaN(price) || price <= 0) {
                throw new Error("Giá sản phẩm phải là số lớn hơn 0");
            }

            const response = await axios.post(
                `http://localhost:5084/api/product/${form.shopId}/CreateProduct`,
                {
                    Name: form.name.trim(),
                    Price: price,
                    Description: form.description?.trim()
                },
                {
                    headers: {
                        Authorization: `Bearer ${token}`,
                        "Content-Type": "application/json"
                    }
                }
            );

            navigate(`/dashboard`, {
                state: {
                    successMessage: `Tạo sản phẩm "${response.data.Name}" thành công!`,
                    newProduct: response.data
                }
            });
        } catch (err) {
            if (err.response?.status === 409) {
                setError(`Lỗi: ${err.response.data.Message || "Tên sản phẩm đã tồn tại trong cửa hàng này"}`);
            } else {
                setError(err.response?.data?.Message || err.message || "Tạo sản phẩm thất bại");
            }
            console.error("Chi tiết lỗi:", err.response?.data || err);
        } finally {
            setIsLoading(false);
        }
    };
    return (
        <div className="min-h-screen bg-gray-50 flex flex-col justify-center py-12 sm:px-6 lg:px-8">
            <div className="sm:mx-auto sm:w-full sm:max-w-md">
                <h2 className="mt-6 text-center text-3xl font-extrabold text-gray-900">
                    {shopId ? "Thêm sản phẩm mới" : "Tạo sản phẩm mới"}
                </h2>
            </div>

            <div className="mt-8 sm:mx-auto sm:w-full sm:max-w-md">
                <div className="bg-white py-8 px-4 shadow sm:rounded-lg sm:px-10">
                    {error && (
                        <div className="mb-4 bg-red-50 border-l-4 border-red-500 p-4">
                            <div className="flex">
                                <div className="text-red-500">{error}</div>
                            </div>
                        </div>
                    )}

                    <form className="space-y-6" onSubmit={handleSubmit}>
                        {!shopId && (
                            <div>
                                <label htmlFor="shopId" className="block text-sm font-medium text-gray-700">
                                    Chọn cửa hàng *
                                </label>
                                <div className="mt-1">
                                    <select
                                        id="shopId"
                                        name="shopId"
                                        value={form.shopId}
                                        onChange={handleChange}
                                        className="appearance-none block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                                        required
                                        disabled={isShopLoading}
                                    >
                                        <option value="">-- Chọn cửa hàng --</option>
                                        {shops.map((shop) => (
                                            <option key={shop.id} value={shop.id}>
                                                {shop.name}
                                            </option>
                                        ))}
                                    </select>
                                    {isShopLoading && (
                                        <p className="mt-1 text-sm text-gray-500">Đang tải danh sách cửa hàng...</p>
                                    )}
                                </div>
                            </div>
                        )}

                        <div>
                            <label htmlFor="name" className="block text-sm font-medium text-gray-700">
                                Tên sản phẩm *
                            </label>
                            <div className="mt-1">
                                <input
                                    id="name"
                                    name="name"
                                    type="text"
                                    required
                                    className="appearance-none block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                                    value={form.name}
                                    onChange={handleChange}
                                    maxLength={100}
                                />
                            </div>
                        </div>

                        <div>
                            <label htmlFor="price" className="block text-sm font-medium text-gray-700">
                                Giá sản phẩm (VNĐ) *
                            </label>
                            <div className="mt-1 relative rounded-md shadow-sm">
                                <input
                                    id="price"
                                    name="price"
                                    type="number"
                                    min="0"
                                    step="0.01"
                                    required
                                    className="appearance-none block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                                    value={form.price}
                                    onChange={handleChange}
                                />
                            </div>
                        </div>

                        <div>
                            <label htmlFor="description" className="block text-sm font-medium text-gray-700">
                                Mô tả sản phẩm
                            </label>
                            <div className="mt-1">
                                <textarea
                                    id="description"
                                    name="description"
                                    rows={3}
                                    className="appearance-none block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                                    value={form.description}
                                    onChange={handleChange}
                                    maxLength={500}
                                />
                            </div>
                        </div>

                        <div>
                            <button
                                type="submit"
                                disabled={isLoading || isShopLoading}
                                className={`w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500 ${isLoading || isShopLoading ? "opacity-50 cursor-not-allowed" : ""
                                    }`}
                            >
                                {isLoading ? (
                                    <>
                                        <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                        </svg>
                                        Đang xử lý...
                                    </>
                                ) : "Tạo sản phẩm"}
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    );
}

export default CreateProduct;