
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { jwtDecode } from "jwt-decode";
import axios from "axios";

function Dashboard() {
    const navigate = useNavigate();
    const token = localStorage.getItem("accessToken");
    const [shops, setShops] = useState([]);
    const [selectedShop, setSelectedShop] = useState(null);
    const [products, setProducts] = useState([]);
    const [loading, setLoading] = useState(false);
    const [searchTerm, setSearchTerm] = useState("");
    const [currentPage, setCurrentPage] = useState(1);
    const productsPerPage = 5;

    const decoded = token ? jwtDecode(token) : null;
    const userId = decoded?.nameid || decoded?.sub;


    useEffect(() => {
        if (!token) {
            navigate("/login");
        } else {
            fetchUserShops();
        }
    }, [token, navigate]);

    const fetchUserShops = async () => {
        try {
            setLoading(true);
            const response = await axios.get(`http://localhost:5084/api/shop/MyShopsList`, {
                headers: { Authorization: `Bearer ${token}` }
            });
            setShops(response.data);
        } catch (error) {
            console.error("Lỗi khi lấy danh sách shop:", error);
        } finally {
            setLoading(false);
        }
    };

    const fetchShopProducts = async (shopId) => {
        try {
            setLoading(true);
            const response = await axios.get(`http://localhost:5084/api/product/${shopId}/Products`, {
                headers: { Authorization: `Bearer ${token}` }
            });

            console.log("API Response:", response.data);

            // Sửa lại theo đúng cấu trúc response (shopId, shopName, shopDescription viết thường)
            setSelectedShop({
                shopId: response.data.shopId,  // sửa từ ShopId -> shopId
                ShopName: response.data.shopName,  // sửa từ ShopName -> shopName
                ShopDescription: response.data.shopDescription  // sửa từ ShopDescription -> shopDescription
            });
            setProducts(response.data.products || []);  // sửa từ Products -> products
        } catch (error) {
            console.error("Lỗi khi lấy sản phẩm:", error);
        } finally {
            setLoading(false);
        }
    };

    const handleDeleteProduct = async (productId) => {
        if (window.confirm("Bạn có chắc muốn xóa sản phẩm này?")) {
            try {
                await axios.delete(`http://localhost:5084/api/product/${productId}/DeleteProduct`, {
                    headers: { Authorization: `Bearer ${token}` }
                });
                setProducts(products.filter(p => p.ProductId !== productId));
            } catch (error) {
                console.error("Lỗi khi xóa sản phẩm:", error);
            }
        }
    };

    const handleLogout = () => {
        localStorage.removeItem("accessToken");
        localStorage.removeItem("refreshToken");
        navigate("/login");
    };

    const filteredProducts = products.filter((product) =>
        product.productName.toLowerCase().includes(searchTerm.toLowerCase())
           
    );

    const indexOfLastProduct = currentPage * productsPerPage;
    const indexOfFirstProduct = indexOfLastProduct - productsPerPage;
    const currentProducts = filteredProducts.slice(indexOfFirstProduct, indexOfLastProduct);
    const totalPages = Math.ceil(filteredProducts.length / productsPerPage);

    return (
        <div className="min-h-screen bg-gray-100">
            {/* Header */}
            <header className="bg-white shadow">
                <div className="max-w-7xl mx-auto py-4 px-4 sm:px-6 lg:px-8 flex justify-between items-center">
                    <h1 className="text-2xl font-bold text-gray-900">Quản lý cửa hàng</h1>
                    <button
                        onClick={handleLogout}
                        className="bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700"
                    >
                        Đăng xuất
                    </button>
                </div>
            </header>

            <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
                {loading && (
                    <div className="text-center py-4">
                        <div className="inline-block animate-spin rounded-full h-8 w-8 border-t-2 border-b-2 border-blue-500"></div>
                    </div>
                )}

                {!selectedShop ? (

                    <div className="bg-white shadow rounded-lg p-6">
                        <div className="flex justify-between items-center mb-4">
                            <h2 className="text-xl font-semibold">Danh sách cửa hàng của bạn</h2>
                            <button
                                onClick={() => navigate("/shop/create")}
                                className="bg-blue-600 text-white px-3 py-1 rounded hover:bg-blue-700"
                            >
                                + Tạo cửa hàng mới
                            </button>
                        </div>

                        {shops.length === 0 ? (
                            <p className="text-gray-500">Bạn chưa có cửa hàng nào.</p>
                        ) : (
                            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                                {shops.map(shop => (
                                    <div
                                        key={shop.id}
                                        className="border rounded-lg p-4 hover:shadow-md cursor-pointer transition-shadow"
                                        onClick={() => fetchShopProducts(shop.id)}
                                    >
                                        <h3 className="font-bold text-lg">{shop.name}</h3>
                                        <p className="text-sm text-gray-500 mt-2">{shop.description}</p>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                ) : (

                    <div className="bg-white shadow rounded-lg p-6">
                        <div className="flex justify-between items-center mb-4">
                            <div>
                                <h2 className="text-xl font-semibold">
                                    Sản phẩm của: {selectedShop.ShopName}
                                </h2>
                                <p className="text-sm text-gray-500">{selectedShop.ShopDescription}</p>
                            </div>
                            <div className="flex space-x-2">
                                <button
                                    onClick={() => setSelectedShop(null)}
                                    className="bg-gray-200 px-3 py-1 rounded hover:bg-gray-300"
                                >
                                    Quay lại
                                </button>
                                <button
                                    onClick={() => navigate(`/shop/${selectedShop.shopId}/product/create`)}
                                    className="bg-green-600 text-white px-3 py-1 rounded hover:bg-green-700"
                                >
                                    + Thêm sản phẩm
                                </button>
                            </div>
                        </div>

                        { }
                        <div className="mb-4">
                            <input
                                type="text"
                                placeholder="Tìm kiếm sản phẩm..."
                                className="w-full p-2 border rounded"
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                            />
                        </div>

                        { }
                        <div className="overflow-x-auto">
                            <table className="min-w-full divide-y divide-gray-200">
                                <thead className="bg-gray-50">
                                    <tr>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Tên sản phẩm</th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Giá</th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Mô tả</th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Thao tác</th>
                                    </tr>
                                </thead>
                                    <tbody className="bg-white divide-y divide-gray-200">
                                        {products && products.length > 0 ? (
                                            currentProducts.map(product => (
                                                <tr key={product.productId || product.id}>
                                                    <td className="px-6 py-4 whitespace-nowrap">{product.productName || product.name}</td>
                                                    <td className="px-6 py-4 whitespace-nowrap">{product.price?.toLocaleString()} VNĐ</td>
                                                    <td className="px-6 py-4">{product.description?.substring(0, 50)}...</td>
                                                    <td className="px-6 py-4 whitespace-nowrap space-x-2">
                                                        <button
                                                            onClick={() => navigate(`/product/${product.productId || product.id}/edit`)}
                                                            className="text-blue-600 hover:text-blue-900"
                                                        >
                                                            Sửa
                                                        </button>
                                                        <button
                                                            onClick={() => handleDeleteProduct(product.productId || product.id)}
                                                            className="text-red-600 hover:text-red-900"
                                                        >
                                                            Xóa
                                                        </button>
                                                    </td>
                                                </tr>
                                            ))
                                        ) : (
                                            <tr>
                                                <td colSpan="4" className="px-6 py-4 text-center">
                                                    {products ? "Không có sản phẩm nào" : "Đang tải..."}
                                                </td>
                                            </tr>
                                        )}
                                    </tbody>
                            </table>
                        </div>

                        { }
                        {totalPages > 1 && (
                            <div className="mt-4 flex justify-center">
                                <button
                                    disabled={currentPage === 1}
                                    onClick={() => setCurrentPage(p => p - 1)}
                                    className="mx-1 px-3 py-1 rounded bg-gray-200 disabled:opacity-50"
                                >
                                    &lt;
                                </button>

                                {Array.from({ length: totalPages }, (_, i) => i + 1).map(page => (
                                    <button
                                        key={page}
                                        onClick={() => setCurrentPage(page)}
                                        className={`mx-1 px-3 py-1 rounded ${currentPage === page ? 'bg-blue-600 text-white' : 'bg-gray-200'}`}
                                    >
                                        {page}
                                    </button>
                                ))}

                                <button
                                    disabled={currentPage === totalPages}
                                    onClick={() => setCurrentPage(p => p + 1)}
                                    className="mx-1 px-3 py-1 rounded bg-gray-200 disabled:opacity-50"
                                >
                                    &gt;
                                </button>
                            </div>
                        )}
                    </div>
                )}
            </main>
        </div>
    );


}

export default Dashboard;
