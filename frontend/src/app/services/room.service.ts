import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Room, PriceSuggestion } from '../models/room.model';

@Injectable({
  providedIn: 'root'
})
export class RoomService {
  private apiUrl = 'http://localhost:5081/api';

  constructor(private http: HttpClient) { }

  getRooms(hotelId?: number | null): Observable<Room[]> {
    if (hotelId) {
      return this.http.get<Room[]>(`${this.apiUrl}/rooms?hotelId=${hotelId}`);
    }
    return this.http.get<Room[]>(`${this.apiUrl}/rooms`);
  }

  getRoom(id: number): Observable<Room> {
    return this.http.get<Room>(`${this.apiUrl}/rooms/${id}`);
  }

  getSuggestedPrice(roomId: number, date?: string): Observable<PriceSuggestion> {
    const url = date 
      ? `${this.apiUrl}/pricing/suggest/${roomId}?date=${date}`
      : `${this.apiUrl}/pricing/suggest/${roomId}`;
    return this.http.get<PriceSuggestion>(url);
  }

  applyPrice(roomId: number, newPrice: number, effectiveDate: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/pricing/apply`, {
      roomId,
      newPrice,
      effectiveDate
    });
  }
}