import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class RoomManagementService {
  private apiUrl = 'http://localhost:5081/api';

  constructor(private http: HttpClient) { }

  getHotels(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/hotels`);
  }

  createHotel(payload: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/hotels`, payload);
  }

  getHotel(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/hotels/${id}`);
  }

  updateHotel(id: number, payload: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/hotels/${id}`, payload);
  }

  deleteHotel(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/hotels/${id}`);
  }

  getRooms(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/rooms`);
  }

  getRoom(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/rooms/${id}`);
  }

  createRoom(payload: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/rooms`, payload);
  }

  updateRoom(id: number, payload: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/rooms/${id}`, payload);
  }

  deleteRoom(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/rooms/${id}`);
  }
}
