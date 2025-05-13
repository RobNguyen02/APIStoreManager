import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { useParams, useNavigate } from 'react-router-dom';

const UpdateProduct = () => {
    const { productId } = useParams();
    const navigate = useNavigate();
    const token = localStorage.getItem('accessToken');

    const [form, setForm] = useState({
        name: '',
        price: '',
        description: '',
        shopId: ''
    });

    const [error, setError] = useState('');
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        const fetchProduct = async () => {
            try {
                const res = await axios.get(
                    `http://localhost:5084/api/product/${productId}/product-details`,
                    {
                        headers: { Authorization: `Bearer ${token}` }
                    }
                );

                setForm({
                    name: res.data.name,
                    price: res.data.price.toString(),
                    description: res.data.description || '',
                    shopId: res.data.shopId
                });
            } catch (err) {
                setError('Không thể tải sản phẩm. Có thể bạn không có quyền hoặc sản phẩm không tồn tại.');
                console.error("Lỗi khi tải sản phẩm:", err);
            }
        };

        if (productId && token) {
            fetchProduct();
        } else {
            navigate('/login');
        }
    }, [productId, token, navigate]);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setForm(prev => ({
            ...prev,
            [name]: value
        }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setIsLoading(true);
        setError('');

        try {
            const price = parseFloat(form.price);
            if (isNaN(price) || price <= 0) {
                throw new Error("Giá sản phẩm phải là số lớn hơn 0");
            }

            const payload = {
                Name: form.name.trim(),
                Price: price,
                Description: form.description?.trim() || null
            };

            await axios.put(
                `http://localhost:5084/api/product/${productId}/UpdateProduct`,
                payload,
                {
                    headers: {
                        Authorization: `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    }
                }
            );

            navigate(`/dashboard`, {
                state: {
                    successMessage: `Sản phẩm "${form.name}" đã được cập nhật thành công!`
                }
            });

        } catch (err) {
            if (err.response?.status === 403) {
                setError("Bạn không có quyền chỉnh sửa sản phẩm này");
            } else if (err.response?.status === 404) {
                setError("Sản phẩm không tồn tại");
            } else if (err.response?.data?.errors) {
                const validationErrors = Object.values(err.response.data.errors).flat().join('\n');
                setError(`Lỗi dữ liệu:\n${validationErrors}`);
            } else {
                setError(err.response?.data?.Message || err.message || "Cập nhật sản phẩm thất bại");
            }
            console.error("Lỗi khi cập nhật sản phẩm:", err);
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="max-w-md mx-auto mt-10">
            <h2 className="text-xl font-bold mb-4">Cập nhật sản phẩm</h2>
            {error && <div className="text-red-500 mb-2 whitespace-pre-line">{error}</div>}

            <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                    <label className="block">Tên sản phẩm *</label>
                    <input
                        type="text"
                        name="name"
                        value={form.name}
                        onChange={handleChange}
                        className="w-full border p-2 rounded"
                        required
                        maxLength={200}
                    />
                </div>

                <div>
                    <label className="block">Giá (VNĐ) *</label>
                    <input
                        type="number"
                        name="price"
                        value={form.price}
                        onChange={handleChange}
                        className="w-full border p-2 rounded"
                        required
                        min="0.01"
                        step="any"
                    />
                </div>

                <div>
                    <label className="block">Mô tả</label>
                    <textarea
                        name="description"
                        value={form.description}
                        onChange={handleChange}
                        className="w-full border p-2 rounded"
                        rows="4"
                        maxLength={1000}
                    />
                </div>

                <div className="flex space-x-3">
                    <button
                        type="button"
                        onClick={() => navigate(-1)}
                        className="w-1/2 bg-gray-200 text-gray-800 px-4 py-2 rounded"
                    >
                        Hủy
                    </button>
                    <button
                        type="submit"
                        disabled={isLoading}
                        className={`w-1/2 bg-blue-500 text-white px-4 py-2 rounded ${isLoading ? 'opacity-50' : ''}`}
                    >
                        {isLoading ? 'Đang lưu...' : 'Cập nhật'}
                    </button>
                </div>
            </form>
        </div>
    );
};

export default UpdateProduct;