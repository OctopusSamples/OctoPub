export interface Products {
    data: ProductData[]
}

export interface Product {
    data: ProductData
}

export interface ProductData {
    id: number | null,
    type: string | null,
    attributes: {
        partition: string | null,
        name: string | null,
        image: string | null,
        pdf: string | null,
        epub: string | null,
        description: string | null
    }
}