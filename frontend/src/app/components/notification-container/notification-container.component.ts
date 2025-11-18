import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService, Notification } from '../../services/notification.service';

@Component({
  selector: 'app-notification-container',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="notification-container">
      <div *ngFor="let notif of notifications" 
           [class]="'notification notification-' + notif.type"
           [@slideIn]>
        <div class="notification-header">
          <h4>{{ notif.title }}</h4>
          <button class="close-btn" (click)="removeNotif(notif.id)" *ngIf="notif.type !== 'confirm'">
            ×
          </button>
        </div>
        <p>{{ notif.message }}</p>
        <div class="notification-actions" *ngIf="notif.type === 'confirm'">
          <button class="btn btn-primary" (click)="notif.confirmCallback?.(true)">Sí, Confirmar</button>
          <button class="btn btn-secondary" (click)="notif.confirmCallback?.(false)">Cancelar</button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .notification-container {
      position: fixed;
      top: 80px;
      right: 20px;
      z-index: 9999;
      max-width: 400px;
      pointer-events: none;
    }

    .notification {
      background: white;
      border-radius: 8px;
      padding: 16px;
      margin-bottom: 12px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
      pointer-events: auto;
      animation: slideIn 0.3s ease-out;
    }

    @keyframes slideIn {
      from {
        transform: translateX(500px);
        opacity: 0;
      }
      to {
        transform: translateX(0);
        opacity: 1;
      }
    }

    .notification-success {
      border-left: 4px solid #28a745;
      background: #f0fdf4;
    }

    .notification-error {
      border-left: 4px solid #dc3545;
      background: #fef2f2;
    }

    .notification-info {
      border-left: 4px solid #17a2b8;
      background: #f0f9fc;
    }

    .notification-confirm {
      border-left: 4px solid #ffc107;
      background: #fffbf0;
    }

    .notification-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 8px;
    }

    .notification-header h4 {
      margin: 0;
      font-size: 16px;
      font-weight: 600;
    }

    .close-btn {
      background: none;
      border: none;
      font-size: 24px;
      cursor: pointer;
      color: #999;
      padding: 0;
      width: 30px;
      height: 30px;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .close-btn:hover {
      color: #333;
    }

    .notification p {
      margin: 8px 0 0 0;
      font-size: 14px;
      color: #333;
    }

    .notification-actions {
      display: flex;
      gap: 8px;
      margin-top: 12px;
    }

    .btn {
      padding: 8px 16px;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-size: 14px;
      font-weight: 500;
    }

    .btn-primary {
      background: #007bff;
      color: white;
    }

    .btn-primary:hover {
      background: #0056b3;
    }

    .btn-secondary {
      background: #6c757d;
      color: white;
    }

    .btn-secondary:hover {
      background: #545b62;
    }
  `]
})
export class NotificationContainerComponent implements OnInit {
  notifications: Notification[] = [];

  constructor(private notificationService: NotificationService) {}

  ngOnInit() {
    this.notificationService.notifications.subscribe(notifs => {
      this.notifications = notifs;
    });
  }

  removeNotif(id: string) {
    this.notifications = this.notifications.filter(n => n.id !== id);
  }
}
