import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AdminNavbarComponent } from '../admin-navbar/admin-navbar.component';
import { AdminService } from '../../services/admin.service';
import { AdminUser, AdminInterview, AdminAssessment, AdminSubscription, AdminPayment } from '../../models/admin.models';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, AdminNavbarComponent],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.css'
})
export class AdminDashboardComponent implements OnInit {
  isLoading = true;
  error = '';

  totalUsers = 0;
  totalInterviews = 0;
  totalAssessments = 0;
  activeSubscriptions = 0;

  recentUsers: AdminUser[] = [];
  recentPayments: AdminPayment[] = [];

  constructor(private adminService: AdminService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    forkJoin({
      users: this.adminService.getUsers(),
      interviews: this.adminService.getAllInterviews(),
      assessments: this.adminService.getAllAssessments(),
      subscriptions: this.adminService.getAllSubscriptions()
    }).subscribe({
      next: (data) => {
        this.totalUsers = data.users?.length || 0;
        this.totalInterviews = data.interviews?.length || 0;
        this.totalAssessments = data.assessments?.length || 0;
        this.activeSubscriptions = (data.subscriptions || []).filter(s => s.status === 'Active').length;

        this.recentUsers = (data.users || [])
          .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
          .slice(0, 10);

        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.error = err?.error?.message || 'Failed to load dashboard data.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });

    this.adminService.getAllPayments().subscribe({
      next: (payments) => {
        this.recentPayments = (payments || [])
          .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
          .slice(0, 5);
        this.cdr.detectChanges();
      },
      error: () => { /* payments are supplementary, don't block dashboard */ }
    });
  }
}
