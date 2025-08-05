import { Product } from "../Product/Product";

export interface ProductStatisticResponse {
    Product: Product;
    TotalSales: number;
}