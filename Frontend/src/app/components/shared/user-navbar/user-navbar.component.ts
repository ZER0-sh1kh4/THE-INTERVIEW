import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { catchError, Observable, of } from 'rxjs';
import { User } from '../../../models/user.model';
import { AuthService } from '../../../services/auth.service';
import { NotificationService } from '../../../services/notification.service';
import { Notification } from '../../../models/notification.model';

@Component({
  selector: 'app-user-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './user-navbar.component.html',
  styleUrl: './user-navbar.component.css'
})
export class UserNavbarComponent {
  currentUser$: Observable<User | null | undefined>;
  notifications$: Observable<Notification[]>;
  unreadCount$: Observable<number>;
  showNotifications = false;

  constructor(
    private authService: AuthService, 
    private router: Router,
    private notificationService: NotificationService
  ) {
    this.currentUser$ = this.authService.currentUser$;
    this.notifications$ = this.notificationService.notifications$;
    this.unreadCount$ = this.notificationService.unreadCount$;
    
    this.notificationService.fetchNotifications().pipe(
      catchError(() => of([] as Notification[]))
    ).subscribe();
  }

  toggleNotifications(): void {
    this.showNotifications = !this.showNotifications;
    if (this.showNotifications) {
      this.notificationService.fetchNotifications().pipe(
        catchError(() => of([] as Notification[]))
      ).subscribe(() => {
        this.notificationService.markAllAsRead().pipe(
          catchError(() => of(null))
        ).subscribe();
      });
    }
  }

  onNotificationClick(note: Notification): void {
    if (note.actionUrl) {
      this.router.navigateByUrl(note.actionUrl);
      this.showNotifications = false;
    }
  }

  getDisplayName(user: User | null): string {
    if (!user?.email) return 'Candidate';
    return user.email.split('@')[0].replace(/[._-]+/g, ' ');
  }

  getPlanLabel(user: User | null): string {
    return user?.isPremium === true ? 'Premium' : 'Free';
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
