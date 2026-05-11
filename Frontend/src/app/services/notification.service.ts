import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, map, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from '../models/user.model';
import { Notification } from '../models/notification.model';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private apiUrl = `${environment.apiUrl}/auth/notifications`;
  
  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  public notifications$ = this.notificationsSubject.asObservable();
  
  private unreadCountSubject = new BehaviorSubject<number>(0);
  public unreadCount$ = this.unreadCountSubject.asObservable();

  constructor(private http: HttpClient) {}

  private unwrapData<T>(response: any): T {
    return (response?.data ?? response?.Data ?? response) as T;
  }

  private normalizeArray<T>(value: any): T[] {
    if (Array.isArray(value)) {
      return value;
    }

    if (Array.isArray(value?.$values)) {
      return value.$values;
    }

    return [];
  }

  private normalizeNotification(raw: any): Notification {
    return {
      id: raw?.id ?? raw?.Id ?? 0,
      userId: raw?.userId ?? raw?.UserId ?? 0,
      title: raw?.title ?? raw?.Title ?? '',
      message: raw?.message ?? raw?.Message ?? '',
      actionUrl: raw?.actionUrl ?? raw?.ActionUrl,
      isRead: raw?.isRead ?? raw?.IsRead ?? false,
      createdAt: raw?.createdAt ?? raw?.CreatedAt ?? ''
    };
  }

  fetchNotifications(): Observable<Notification[]> {
    return this.http.get<ApiResponse<Notification[]>>(this.apiUrl).pipe(
      map(res => this.normalizeArray<any>(this.unwrapData<any>(res)).map(note => this.normalizeNotification(note))),
      tap(notifications => {
        this.notificationsSubject.next(notifications);
        this.unreadCountSubject.next(notifications.filter(n => !n.isRead).length);
      })
    );
  }

  markAllAsRead(): Observable<any> {
    return this.http.put(`${this.apiUrl}/read`, {}).pipe(
      tap(() => {
        const current = this.notificationsSubject.value.map(n => ({ ...n, isRead: true }));
        this.notificationsSubject.next(current);
        this.unreadCountSubject.next(0);
      })
    );
  }
}
