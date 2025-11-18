import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../services/notification.service';
import { HotelService, Hotel } from '../../services/hotel.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  email = '';
  password = '';
  confirmPassword = '';
  fullName = '';
  hotelId: number | null = null;
  errorMessage = '';
  isLoading = false;
  hotels: Hotel[] = [];

  // Validación mínima en frontend: solo dominio corporativo permitido
  readonly allowedDomain = '@posadas.com';

  constructor(
    private auth: AuthService,
    private notificationService: NotificationService,
    private hotelService: HotelService,
    private router: Router
  ) {
    this.resetForm();
  }

  ngOnInit() {
    this.loadHotels();
  }

  loadHotels() {
    this.hotelService.getHotels().subscribe({
      next: (hotels) => {
        this.hotels = hotels;
      },
      error: (err) => {
        console.error('Error cargando hoteles:', err);
        this.errorMessage = 'Error al cargar la lista de hoteles';
      }
    });
  }

  resetForm() {
    this.email = '';
    this.password = '';
    this.confirmPassword = '';
    this.fullName = '';
    this.hotelId = null;
    this.errorMessage = '';
  }

  onSubmit() {
    this.errorMessage = '';
    if (!this.fullName || this.fullName.trim().length < 2) {
      this.errorMessage = 'Por favor ingresa tu nombre completo';
      return;
    }

    if (!this.email) {
      this.errorMessage = 'Por favor ingresa un email';
      return;
    }

    if (!this.email.toLowerCase().endsWith(this.allowedDomain)) {
      this.errorMessage = `El email debe pertenecer al dominio ${this.allowedDomain}`;
      return;
    }

    if (!this.password || this.password.length < 8) {
      this.errorMessage = 'La contraseña debe tener al menos 8 caracteres';
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.errorMessage = 'Las contraseñas no coinciden';
      return;
    }

    if (!this.hotelId) {
      this.errorMessage = 'Por favor selecciona un hotel';
      return;
    }

    this.isLoading = true;
    this.auth.register({ 
      email: this.email, 
      password: this.password, 
      fullName: this.fullName,
      hotelId: this.hotelId
    } as any)
      .subscribe({
        next: () => {
          this.isLoading = false;
          this.notificationService.success('Usuario creado. Inicia sesión con tus credenciales.');
          this.router.navigate(['/login']);
        },
        error: (err) => {
          this.isLoading = false;
          console.error('Error al registrar:', err);
          this.errorMessage = err?.error?.message || 'Error al registrar usuario';
        }
      });
  }
}
