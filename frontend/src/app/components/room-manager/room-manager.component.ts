import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RoomManagementService } from '../../services/room-management.service';
import { NotificationService } from '../../services/notification.service';

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
  // Estado de edición
  editingRoomId: number | null = null;
  editModel: any = {
    basePrice: 0,
    capacity: 1,
    hasBalcony: false,
    hasSeaView: false
  };

  // Formulario de creación
  showForm = false;
  newRoom = {
    hotelId: 1,
    roomNumber: '',
    type: 'Single',
    basePrice: 0,
    capacity: 1,
    hasBalcony: false,
    hasSeaView: false,
    squareMeters: 0
  };

  roomTypes = ['Single', 'Double', 'Suite', 'Deluxe', 'Presidential'];

  constructor(private roomService: RoomManagementService, private notificationService: NotificationService) {}

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

    const roomPayload = {
      hotelId: this.newRoom.hotelId,
      roomNumber: this.newRoom.roomNumber,
      type: this.newRoom.type,
      basePrice: this.newRoom.basePrice,
      capacity: this.newRoom.capacity,
      hasBalcony: this.newRoom.hasBalcony,
      hasSeaView: this.newRoom.hasSeaView,
      squareMeters: this.newRoom.squareMeters
    };

    this.roomService.createRoom(roomPayload).subscribe({
      next: () => {
        this.successMessage = 'Habitación creada exitosamente';
        this.loadRooms();
        this.resetForm();
        this.showForm = false;
      },
      error: (err) => {
        console.error('Error al crear habitación:', err);
        this.errorMessage = 'Error al crear habitación';
      }
    });
  }

  deleteRoom(room: any): void {
    this.notificationService.confirm(
      `¿Eliminar habitación ${room.roomNumber}?`,
      (confirmed) => {
        if (!confirmed) return;

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
      },
      '⚠️ Confirmar eliminación'
    );
  }

  startEdit(room: any): void {
    this.editingRoomId = room.id;
    this.editModel = {
      basePrice: room.basePrice,
      capacity: room.capacity,
      hasBalcony: room.hasBalcony ?? false,
      hasSeaView: room.hasSeaView ?? false
    };
  }

  cancelEdit(): void {
    this.editingRoomId = null;
    this.editModel = {
      basePrice: 0,
      capacity: 1,
      hasBalcony: false,
      hasSeaView: false
    };
  }

  saveEdit(room: any): void {
    // Validación básica
    if (this.editModel.basePrice <= 0 || this.editModel.capacity <= 0) {
      this.errorMessage = 'Precio base y capacidad deben ser mayores a 0';
      return;
    }

    const payload = {
      basePrice: this.editModel.basePrice,
      capacity: this.editModel.capacity,
      hasBalcony: this.editModel.hasBalcony,
      hasSeaView: this.editModel.hasSeaView
    };

    this.roomService.updateRoom(room.id, payload).subscribe({
      next: () => {
        this.successMessage = 'Habitación actualizada';
        // actualizar en la lista localmente para evitar recarga completa
        const idx = this.rooms.findIndex(r => r.id === room.id);
        if (idx !== -1) {
          this.rooms[idx].basePrice = this.editModel.basePrice;
          this.rooms[idx].capacity = this.editModel.capacity;
          this.rooms[idx].hasBalcony = this.editModel.hasBalcony;
          this.rooms[idx].hasSeaView = this.editModel.hasSeaView;
        }
        this.cancelEdit();
      },
      error: (err) => {
        console.error('Error al actualizar habitación:', err);
        this.errorMessage = 'Error al actualizar habitación';
      }
    });
  }
}
