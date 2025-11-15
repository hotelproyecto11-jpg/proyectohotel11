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
  role = 'StaffOperativo';
  errorMessage = '';
  isLoading = false;

  roles = [
    { value: 'StaffOperativo', label: 'Staff Operativo' },
    { value: 'GerenteComercial', label: 'Gerente Comercial' },
    { value: 'RevenueManager', label: 'Revenue Manager' }
  ];

  constructor(private auth: AuthService, private router: Router) {}

  onSubmit() {
    this.errorMessage = '';
    if (!this.email || !this.password || !this.fullName) {
      this.errorMessage = 'Por favor completa todos los campos';
      return;
    }

    this.isLoading = true;
    this.auth.register({ email: this.email, password: this.password, fullName: this.fullName, role: this.role })
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
