export interface Products {
    data: ProductData[]
}

export interface Product {
    data: ProductData
}

export interface ProductData {
    id: number,
    attributes: {
        tenant: string,
        name: string,
        image: string,
        pdf: string,
        epub: string,
        description: string
    }
}