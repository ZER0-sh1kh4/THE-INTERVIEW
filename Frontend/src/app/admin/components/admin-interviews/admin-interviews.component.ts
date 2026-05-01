import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminNavbarComponent } from '../admin-navbar/admin-navbar.component';
import { AdminService } from '../../services/admin.service';
import { AdminInterview } from '../../models/admin.models';

@Component({
  selector: 'app-admin-interviews',
  standalone: true,
  imports: [CommonModule, FormsModule, AdminNavbarComponent],
  templateUrl: './admin-interviews.component.html',
  styleUrl: './admin-interviews.component.css'
})
export class AdminInterviewsComponent implements OnInit {
  interviews: AdminInterview[] = [];
  filteredInterviews: AdminInterview[] = [];
  searchTerm = '';
  statusFilter = '';
  typeFilter = '';
  isLoading = true;
  error = '';
  selectedInterview: AdminInterview | null = null;

  constructor(private adminService: AdminService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadInterviews();
  }

  loadInterviews(): void {
    this.isLoading = true;
    this.adminService.getAllInterviews().subscribe({
      next: (data) => {
        this.interviews = (data || []).sort((a, b) =>
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        );
        this.filteredInterviews = [...this.interviews];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.error = err?.error?.message || 'Failed to load interviews.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  applyFilters(): void {
    const term = this.searchTerm.toLowerCase();
    this.filteredInterviews = this.interviews.filter(i => {
      const matchesSearch = !term ||
        i.domain?.toLowerCase().includes(term) ||
        i.title?.toLowerCase().includes(term) ||
        i.userId?.toString().includes(term);
      const matchesStatus = !this.statusFilter || i.status === this.statusFilter;
      const matchesType = !this.typeFilter || i.type === this.typeFilter;
      return matchesSearch && matchesStatus && matchesType;
    });
  }

  viewDetails(interview: AdminInterview): void {
    this.selectedInterview = interview;
  }

  closeModal(): void {
    this.selectedInterview = null;
  }
}
