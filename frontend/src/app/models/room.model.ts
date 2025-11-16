export interface Room {
  id: number;
  roomNumber: string;
  type: string;
  basePrice: number;
  suggestedPrice?: number;
  capacity: number;
  hotelName: string;
}

export interface PriceSuggestion {
  roomId: number;
  targetDate: string;
  suggestedPrice: number;
  currency: string;
  basePriceUsed?: number;
  avgOccupancy?: number;
  hadHistory?: boolean;
  priceSource?: string;
}