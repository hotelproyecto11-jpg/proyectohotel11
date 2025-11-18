import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface Notification {
  id: string;
  type: 'success' | 'error' | 'info' | 'confirm';
  message: string;
  title?: string;
  confirmCallback?: (confirmed: boolean) => void;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notifications$ = new BehaviorSubject<Notification[]>([]);
  public notifications = this.notifications$.asObservable();

  success(message: string, title: string = '✅ Éxito') {
    this.show('success', message, title);
  }

  error(message: string, title: string = '❌ Error') {
    this.show('error', message, title);
  }

  info(message: string, title: string = 'ℹ️ Información') {
    this.show('info', message, title);
  }

  confirm(message: string, callback: (confirmed: boolean) => void, title: string = '⚠️ Confirmar') {
    const id = this.generateId();
    const notification: Notification = {
      id,
      type: 'confirm',
      message,
      title,
      confirmCallback: (confirmed) => {
        callback(confirmed);
        this.remove(id);
      }
    };
    const current = this.notifications$.value;
    this.notifications$.next([...current, notification]);
  }

  private show(type: 'success' | 'error' | 'info', message: string, title: string) {
    const id = this.generateId();
    const notification: Notification = {
      id,
      type,
      message,
      title
    };
    const current = this.notifications$.value;
    this.notifications$.next([...current, notification]);

    // Auto-remove después de 5 segundos
    setTimeout(() => this.remove(id), 5000);
  }

  private remove(id: string) {
    const current = this.notifications$.value;
    this.notifications$.next(current.filter(n => n.id !== id));
  }

  private generateId(): string {
    return `notification-${Date.now()}-${Math.random()}`;
  }
}
