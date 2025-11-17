import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../../services/user.service';
import { AuthService } from '../../services/auth.service';
import { RoomManagerComponent } from '../room-manager/room-manager.component';
import { HotelManagerComponent } from '../hotel-manager/hotel-manager.component';

@Component({
  selector: 'app-admin-panel',
  standalone: true,
  imports: [CommonModule, FormsModule, RoomManagerComponent, HotelManagerComponent],
  templateUrl: './admin-panel.component.html',
  styleUrls: ['./admin-panel.component.css']
})
export class AdminPanelComponent implements OnInit {
  users: any[] = [];
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  activeTab: 'users' | 'rooms' | 'hotels' = 'users';

  selectedUser: any = null;
  editingId: number | null = null;
  editForm = { email: '', fullName: '', role: '' };
  roles: string[] = ['Admin','RevenueManager','GerenteComercial','StaffOperativo'];
  showCreateForm = false;
  createForm = { email: '', password: '', fullName: '', role: 'StaffOperativo' };
  // Fallback map in case backend returns numeric enum values
  roleMap: Record<number, string> = {
    1: 'Admin',
    2: 'RevenueManager',
    3: 'GerenteComercial',
    4: 'StaffOperativo'
  };

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.userService.getUsers().subscribe({
      next: (data) => {
        // Normalize role to string name if backend returns numeric enums
        this.users = (data || []).map((u: any) => {
          const rawRole = u?.role;
          let roleName = rawRole;
          if (typeof rawRole === 'number') roleName = this.roleMap[rawRole] ?? String(rawRole);
          if (typeof rawRole === 'string' && /^[0-9]+$/.test(rawRole)) {
            const n = parseInt(rawRole, 10);
            roleName = this.roleMap[n] ?? rawRole;
          }
          return { ...u, role: roleName };
        });
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error al cargar usuarios:', err);
        this.errorMessage = 'Error al cargar usuarios';
        this.isLoading = false;
      }
    });
  }

  selectUser(user: any): void {
    this.selectedUser = user;
    this.editingId = null;
    this.successMessage = '';
    this.errorMessage = '';
  }

  startEdit(user: any): void {
    this.editingId = user.id;
    this.editForm = { email: user.email, fullName: user.fullName, role: user.role };
    this.successMessage = '';
    this.errorMessage = '';
  }

  cancelEdit(): void {
    this.editingId = null;
    this.editForm = { email: '', fullName: '', role: '' };
  }

  saveEdit(user: any): void {
    if (!this.editForm.email || !this.editForm.fullName) {
      this.errorMessage = 'Email y nombre completo son requeridos';
      return;
    }

    this.userService.updateUser(user.id, this.editForm).subscribe({
      next: () => {
        // If role changed, call role endpoint
        if (this.editForm.role && this.editForm.role !== user.role) {
          this.userService.setUserRole(user.id, this.editForm.role).subscribe({
            next: () => {
              this.successMessage = 'Usuario actualizado';
              this.editingId = null;
              this.loadUsers();
            },
            error: (err) => {
              console.error('Error al actualizar role:', err);
              this.errorMessage = 'Error al actualizar role';
            }
          });
        } else {
          this.successMessage = 'Usuario actualizado';
          this.editingId = null;
          this.loadUsers();
        }
      },
      error: (err) => {
        console.error('Error al actualizar:', err);
        this.errorMessage = 'Error al actualizar usuario';
      }
    });
  }

  toggleCreateForm() {
    this.showCreateForm = !this.showCreateForm;
    this.successMessage = '';
    this.errorMessage = '';
  }

  createUser() {
    if (!this.createForm.email || !this.createForm.password || !this.createForm.fullName) {
      this.errorMessage = 'Email, contraseña y nombre completo son requeridos';
      return;
    }

    this.userService.createUser(this.createForm).subscribe({
      next: (res) => {
        this.successMessage = 'Usuario creado';
        this.showCreateForm = false;
        this.createForm = { email: '', password: '', fullName: '', role: 'StaffOperativo' };
        this.loadUsers();
      },
      error: (err) => {
        console.error('Error al crear usuario:', err);
        this.errorMessage = err?.error?.message || 'Error al crear usuario';
      }
    });
  }

  toggleActive(user: any): void {
    this.userService.toggleUserActive(user.id).subscribe({
      next: (res) => {
        this.successMessage = res.message;
        this.loadUsers();
      },
      error: (err) => {
        console.error('Error al cambiar estado:', err);
        this.errorMessage = 'Error al cambiar estado del usuario';
      }
    });
  }

  deleteUser(user: any): void {
    if (!confirm(`¿Eliminar usuario ${user.email}?`)) return;

    this.userService.deleteUser(user.id).subscribe({
      next: () => {
        this.successMessage = 'Usuario eliminado';
        this.loadUsers();
        this.selectedUser = null;
      },
      error: (err) => {
        console.error('Error al eliminar:', err);
        this.errorMessage = err?.error?.message || 'Error al eliminar usuario';
      }
    });
  }

  logout(): void {
    this.authService.logout();
  }

  gotoDashboard(): void {
    this.router.navigate(['/dashboard']);
  }

  setTab(tab: 'users' | 'rooms' | 'hotels') {
    this.activeTab = tab;
    this.successMessage = '';
    this.errorMessage = '';
  }
}
