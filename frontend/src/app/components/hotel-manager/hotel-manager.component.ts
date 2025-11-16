import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RoomManagementService } from '../../services/room-management.service';

@Component({
  selector: 'app-hotel-manager',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './hotel-manager.component.html',
  styleUrls: ['./hotel-manager.component.css']
})
export class HotelManagerComponent implements OnInit {
  isSaving = false;
  errorMessage = '';
  successMessage = '';

  hotels: any[] = [];
  editingId: number | null = null;
  newHotel = {
    name: '',
    city: '',
    state: '',
    address: '',
    stars: 3,
    description: ''
  };

  constructor(private svc: RoomManagementService) {}

  ngOnInit(): void {}

  ngAfterViewInit(): void {
    this.loadHotels();
  }

  loadHotels() {
    this.svc.getHotels().subscribe({
      next: (data) => {
        this.hotels = data;
      },
      error: (err) => {
        console.error('Error al cargar hoteles', err);
        this.errorMessage = 'Error al cargar hoteles';
      }
    });
  }

  selectHotelForEdit(h: any) {
    this.editingId = h.id;
    this.newHotel = { name: h.name || '', city: h.city || '', state: h.state || '', address: h.address || '', stars: h.stars || 3, description: h.description || '' };
    this.successMessage = '';
    this.errorMessage = '';
  }

  cancelEdit() {
    this.editingId = null;
    this.resetForm();
  }

  resetForm() {
    this.newHotel = { name: '', city: '', state: '', address: '', stars: 3, description: '' };
    this.errorMessage = '';
    this.successMessage = '';
  }

  saveHotel() {
    if (!this.newHotel.name || !this.newHotel.city) {
      this.errorMessage = 'Nombre y ciudad son requeridos';
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    this.svc.createHotel(this.newHotel).subscribe({
      next: () => {
        this.successMessage = 'Hotel creado correctamente';
        this.isSaving = false;
        this.resetForm();
        this.loadHotels();
      },
      error: (err) => {
        console.error('Error creando hotel', err);
        this.errorMessage = err?.error?.message || 'Error al crear hotel';
        this.isSaving = false;
      }
    });
  }

  saveEdit() {
    if (!this.editingId) return;
    this.isSaving = true;
    this.svc.updateHotel(this.editingId, this.newHotel).subscribe({
      next: () => {
        this.successMessage = 'Hotel actualizado';
        this.isSaving = false;
        this.editingId = null;
        this.resetForm();
        this.loadHotels();
      },
      error: (err) => {
        console.error('Error actualizando hotel', err);
        this.errorMessage = err?.error?.message || 'Error al actualizar hotel';
        this.isSaving = false;
      }
    });
  }

  deleteHotel(h: any) {
    if (!confirm(`¿Eliminar hotel ${h.name}?`)) return;
    this.svc.deleteHotel(h.id).subscribe({
      next: () => {
        this.successMessage = 'Hotel eliminado';
        this.loadHotels();
      },
      error: (err) => {
        console.error('Error eliminando hotel', err);
        this.errorMessage = err?.error?.message || 'Error al eliminar hotel';
      }
    });
  }
}
