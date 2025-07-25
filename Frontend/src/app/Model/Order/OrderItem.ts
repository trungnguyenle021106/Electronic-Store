import { Product } from "../Product/Product";

export interface OrderItem {
    Product: Product;
    Quantity: number;
    TotalPrice: number;
}