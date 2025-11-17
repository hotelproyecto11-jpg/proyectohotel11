import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  email = '';
  password = '';
  fullName = '';
  errorMessage = '';
  isLoading = false;

  // Validación mínima en frontend: solo dominio corporativo permitido
  readonly allowedDomain = '@posadas.com';

  constructor(private auth: AuthService, private router: Router) {}

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

    this.isLoading = true;
    // Nota: no enviamos rol desde el frontend; el backend asignará el rol por defecto
    this.auth.register({ email: this.email, password: this.password, fullName: this.fullName })
      .subscribe({
        next: () => {
          this.isLoading = false;
          alert('Usuario creado. Inicia sesión con tus credenciales.');
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
