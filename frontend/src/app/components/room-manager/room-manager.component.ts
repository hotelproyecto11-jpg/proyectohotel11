import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RoomManagementService } from '../../services/room-management.service';

@Component({
  selector: 'app-room-manager',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './room-manager.component.html',
  styleUrls: ['./room-manager.component.css']
})
export class RoomManagerComponent implements OnInit {
  rooms: any[] = [];
  hotels: any[] = [];
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  // Formulario de creación
  showForm = false;
  newRoom = {
    hotelId: 1,
    roomNumber: '',
    type: 'Single',
    basePrice: 0,
    capacity: 1,
    quantity: 1,
    hasBalcony: false,
    hasSeaView: false,
    squareMeters: 0
  };

  roomTypes = ['Single', 'Double', 'Suite', 'Deluxe', 'Presidential'];

  constructor(private roomService: RoomManagementService) {}

  ngOnInit(): void {
    this.loadHotels();
    this.loadRooms();
  }

  loadHotels(): void {
    this.roomService.getHotels().subscribe({
      next: (data) => {
        this.hotels = data;
        if (this.hotels.length > 0) {
          const exists = this.hotels.some(h => h.id === this.newRoom.hotelId);
          if (!exists) {
            this.newRoom.hotelId = this.hotels[0].id;
          }
        }
      },
      error: (err) => {
        console.error('Error al cargar hoteles:', err);
        this.errorMessage = 'Error al cargar hoteles';
      }
    });
  }

  loadRooms(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.roomService.getRooms().subscribe({
      next: (data) => {
        this.rooms = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error al cargar habitaciones:', err);
        this.errorMessage = 'Error al cargar habitaciones';
        this.isLoading = false;
      }
    });
  }

  toggleForm(): void {
    this.showForm = !this.showForm;
    if (!this.showForm) {
      this.resetForm();
    }
  }

  resetForm(): void {
    this.newRoom = {
      hotelId: 1,
      roomNumber: '',
      type: 'Single',
      basePrice: 0,
      capacity: 1,
      quantity: 1,
      hasBalcony: false,
      hasSeaView: false,
      squareMeters: 0
    };
    this.errorMessage = '';
    this.successMessage = '';
  }

  saveRoom(): void {
    if (!this.newRoom.roomNumber || this.newRoom.basePrice <= 0 || this.newRoom.capacity <= 0) {
      this.errorMessage = 'Por favor completa los campos requeridos correctamente';
      return;
    }

    // Crear múltiples habitaciones si quantity > 1
    const quantity = this.newRoom.quantity || 1;
    let createdCount = 0;
    let failedCount = 0;

    for (let i = 0; i < quantity; i++) {
      const roomPayload = {
        hotelId: this.newRoom.hotelId,
        roomNumber: this.newRoom.roomNumber + (quantity > 1 ? `-${i + 1}` : ''),
        type: this.newRoom.type,
        basePrice: this.newRoom.basePrice,
        capacity: this.newRoom.capacity,
        quantity: 1,
        hasBalcony: this.newRoom.hasBalcony,
        hasSeaView: this.newRoom.hasSeaView,
        squareMeters: this.newRoom.squareMeters
      };

      this.roomService.createRoom(roomPayload).subscribe({
        next: () => {
          createdCount++;
          if (createdCount + failedCount === quantity) {
            this.successMessage = `${createdCount} habitacion(es) creada(s)`;
            if (failedCount > 0) {
              this.errorMessage = `${failedCount} habitacion(es) fallaron`;
            }
            this.loadRooms();
            this.resetForm();
            this.showForm = false;
          }
        },
        error: (err) => {
          failedCount++;
          console.error('Error al crear habitación:', err);
          if (createdCount + failedCount === quantity) {
            this.successMessage = `${createdCount} habitacion(es) creada(s)`;
            if (failedCount > 0) {
              this.errorMessage = `${failedCount} habitacion(es) fallaron`;
            }
            this.loadRooms();
          }
        }
      });
    }
  }

  deleteRoom(room: any): void {
    if (!confirm(`¿Eliminar habitación ${room.roomNumber}?`)) return;

    this.roomService.deleteRoom(room.id).subscribe({
      next: () => {
        this.successMessage = 'Habitación eliminada';
        this.loadRooms();
      },
      error: (err) => {
        console.error('Error al eliminar:', err);
        this.errorMessage = 'Error al eliminar habitación';
      }
    });
  }
}
