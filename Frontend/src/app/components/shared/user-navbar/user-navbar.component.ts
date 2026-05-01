import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { Observable } from 'rxjs';
import { User } from '../../../models/user.model';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-user-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './user-navbar.component.html',
  styleUrl: './user-navbar.component.css'
})
export class UserNavbarComponent {
  currentUser$: Observable<User | null | undefined>;

  constructor(private authService: AuthService, private router: Router) {
    this.currentUser$ = this.authService.currentUser$;
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
