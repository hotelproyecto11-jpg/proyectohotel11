import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Chart, ChartConfiguration, registerables } from 'chart.js';
import { RoomService } from '../../services/room.service';
import { AuthService } from '../../services/auth.service';
import { Room, PriceSuggestion } from '../../models/room.model';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  rooms: Room[] = [];
  currentUser: any;
  isLoading: boolean = true;
  loadingPrices: { [key: number]: boolean } = {};
  chart: Chart | null = null;

  constructor(
    private roomService: RoomService,
    private authService: AuthService,
    private router: Router
  ) {
    this.currentUser = this.authService.currentUserValue;
  }

  ngOnInit(): void {
    this.loadRooms();
  }

  loadRooms(): void {
    this.isLoading = true;
    this.roomService.getRooms().subscribe({
      next: (rooms) => {
        this.rooms = rooms;
        this.isLoading = false;
        setTimeout(() => this.createChart(), 100);
      },
      error: (error) => {
        console.error('Error al cargar habitaciones:', error);
        this.isLoading = false;
        alert('Error al cargar habitaciones. Verifica tu conexión.');
      }
    });
  }

  getSuggestedPrice(room: Room): void {
    this.loadingPrices[room.id] = true;
    
    this.roomService.getSuggestedPrice(room.id).subscribe({
      next: (suggestion: PriceSuggestion) => {
        const roomIndex = this.rooms.findIndex(r => r.id === room.id);
        if (roomIndex !== -1) {
          this.rooms[roomIndex].suggestedPrice = suggestion.suggestedPrice;
        }
        this.loadingPrices[room.id] = false;
        this.updateChart();
      },
      error: (error) => {
        console.error('Error al obtener precio sugerido:', error);
        this.loadingPrices[room.id] = false;
        alert('Error al calcular precio sugerido');
      }
    });
  }

  applyPrice(room: Room): void {
    if (!room.suggestedPrice) {
      alert('Primero calcula el precio sugerido');
      return;
    }

    if (!confirm(`¿Aplicar precio de $${room.suggestedPrice} MXN para ${room.type}?`)) {
      return;
    }

    const today = new Date().toISOString().split('T')[0];
    
    this.roomService.applyPrice(room.id, room.suggestedPrice, today).subscribe({
      next: () => {
        alert('✅ Precio aplicado exitosamente');
        this.loadRooms(); // Recargar datos
      },
      error: (error) => {
        console.error('Error al aplicar precio:', error);
        alert('❌ Error al aplicar precio');
      }
    });
  }

  getDifference(room: Room): number {
    if (!room.suggestedPrice) return 0;
    return room.suggestedPrice - room.basePrice;
  }

  getDifferencePercent(room: Room): number {
    if (!room.suggestedPrice) return 0;
    return ((room.suggestedPrice - room.basePrice) / room.basePrice) * 100;
  }

  createChart(): void {
    const canvas = document.getElementById('priceChart') as HTMLCanvasElement;
    if (!canvas) return;

    if (this.chart) {
      this.chart.destroy();
    }

    const roomTypes = this.rooms.map(r => r.type);
    const basePrices = this.rooms.map(r => r.basePrice);
    const suggestedPrices = this.rooms.map(r => r.suggestedPrice || r.basePrice);

    const config: ChartConfiguration = {
      type: 'bar',
      data: {
        labels: roomTypes,
        datasets: [
          {
            label: 'Precio Base',
            data: basePrices,
            backgroundColor: 'rgba(54, 162, 235, 0.6)',
            borderColor: 'rgba(54, 162, 235, 1)',
            borderWidth: 2
          },
          {
            label: 'Precio Sugerido',
            data: suggestedPrices,
            backgroundColor: 'rgba(75, 192, 192, 0.6)',
            borderColor: 'rgba(75, 192, 192, 1)',
            borderWidth: 2
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'top',
          },
          title: {
            display: true,
            text: 'Comparación: Precio Base vs Precio Sugerido (MXN)',
            font: {
              size: 16,
              weight: 'bold'
            }
          }
        },
        scales: {
          y: {
            beginAtZero: true,
            ticks: {
              callback: function(value) {
                return '$' + value.toLocaleString();
              }
            }
          }
        }
      }
    };

    this.chart = new Chart(canvas, config);
  }

  updateChart(): void {
    if (this.chart) {
      const suggestedPrices = this.rooms.map(r => r.suggestedPrice || r.basePrice);
      this.chart.data.datasets[1].data = suggestedPrices;
      this.chart.update();
    }
  }

  logout(): void {
    if (confirm('¿Seguro que deseas cerrar sesión?')) {
      this.authService.logout();
    }
  }

  canApplyPrices(): boolean {
    return this.currentUser?.role === 'Admin' || 
           this.currentUser?.role === 'RevenueManager';
  }

  getAveragePrice(): number {
    if (this.rooms.length === 0) return 0;
    const sum = this.rooms.reduce((acc, room) => acc + room.basePrice, 0);
    return sum / this.rooms.length;
  }

  getTotalCapacity(): number {
    return this.rooms.reduce((acc, room) => acc + room.capacity, 0);
  }

  gotoAdminPanel(): void {
    this.router.navigate(['/admin']);
  }
}