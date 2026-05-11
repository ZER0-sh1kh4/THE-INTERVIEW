import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminNavbarComponent } from '../admin-navbar/admin-navbar.component';
import { AdminService } from '../../services/admin.service';
import { AdminUser } from '../../models/admin.models';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, FormsModule, AdminNavbarComponent],
  templateUrl: './admin-users.component.html',
  styleUrl: './admin-users.component.css'
})
export class AdminUsersComponent implements OnInit {
  users: AdminUser[] = [];
  filteredUsers: AdminUser[] = [];
  searchTerm = '';
  isLoading = true;
  error = '';
  premiumCount = 0;

  // Delete dialog
  showDeleteDialog = false;
  deleteTarget: AdminUser | null = null;
  isDeleting = false;

  constructor(
    private adminService: AdminService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.adminService.getUsers().subscribe({
      next: (data) => {
        this.users = data || [];
        this.filteredUsers = [...this.users];
        this.premiumCount = this.users.filter(u => u.isPremium).length;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.error = err?.error?.message || 'Failed to load users.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  filterUsers(): void {
    const term = this.searchTerm.toLowerCase();
    this.filteredUsers = this.users.filter(u =>
      u.fullName?.toLowerCase().includes(term) ||
      u.email?.toLowerCase().includes(term) ||
      u.id?.toString().includes(term)
    );
  }

  /** Toggle role between Admin and Candidate — requires confirmation */
  onRoleToggle(user: AdminUser): void {
    const newRole = user.role === 'Admin' ? 'Candidate' : 'Admin';
    const action = newRole === 'Admin' ? 'promote to ADMIN' : 'demote to CANDIDATE';
    if (confirm(`Are you sure you want to ${action} ${user.fullName || user.email}?`)) {
      this.adminService.updateUserRole(user.id, newRole).subscribe({
        next: () => {
          user.role = newRole;
          this.cdr.detectChanges();
        },
        error: () => {
          alert('Failed to update role. Please try again.');
        }
      });
    }
  }

  /** Inline premium toggle — requires confirmation */
  onPremiumToggle(user: AdminUser, event: Event): void {
    event.stopPropagation();
    const newValue = !user.isPremium;
    if (confirm(`Are you sure you want to ${newValue ? 'grant' : 'revoke'} premium status for ${user.email}?`)) {
      this.adminService.updateUserPremium(user.id, newValue).subscribe({
        next: () => {
          user.isPremium = newValue;
          this.premiumCount = this.users.filter(u => u.isPremium).length;
          this.cdr.detectChanges();
        },
        error: () => {
          // Re-sync UI state on error if needed
          this.cdr.detectChanges();
        }
      });
    }
  }

  /** Returns true if the given user is the currently logged-in admin */
  isSelf(user: AdminUser): boolean {
    const me = this.authService.currentUserValue;
    return me?.userId?.toString() === user.id?.toString();
  }

  promptDelete(user: AdminUser): void {
    if (this.isSelf(user)) {
      alert('You cannot deactivate your own admin account.');
      return;
    }
    this.deleteTarget = user;
    this.showDeleteDialog = true;
  }

  cancelDelete(): void {
    this.deleteTarget = null;
    this.showDeleteDialog = false;
  }

  confirmDelete(): void {
    if (!this.deleteTarget) return;
    this.isDeleting = true;
    this.adminService.deactivateUser(this.deleteTarget.id).subscribe({
      next: () => {
        this.isDeleting = false;
        this.showDeleteDialog = false;
        this.deleteTarget = null;
        this.loadUsers();
      },
      error: () => {
        this.isDeleting = false;
        this.showDeleteDialog = false;
      }
    });
  }

  onReactivate(user: AdminUser): void {
    if (confirm(`Are you sure you want to reactivate ${user.email}?`)) {
      this.adminService.reactivateUser(user.id).subscribe(() => {
        this.loadUsers(); // Refresh the list
      });
    }
  }
}
