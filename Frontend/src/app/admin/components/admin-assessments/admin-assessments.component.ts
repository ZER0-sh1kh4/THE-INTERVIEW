import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminNavbarComponent } from '../admin-navbar/admin-navbar.component';
import { AdminService } from '../../services/admin.service';
import { AdminAssessment } from '../../models/admin.models';

@Component({
  selector: 'app-admin-assessments',
  standalone: true,
  imports: [CommonModule, FormsModule, AdminNavbarComponent],
  templateUrl: './admin-assessments.component.html',
  styleUrl: './admin-assessments.component.css'
})
export class AdminAssessmentsComponent implements OnInit {
  assessments: AdminAssessment[] = [];
  filteredAssessments: AdminAssessment[] = [];
  domains: string[] = [];
  domainFilter = '';
  gradeFilter = '';
  isLoading = true;
  error = '';

  constructor(private adminService: AdminService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadAssessments();
  }

  loadAssessments(): void {
    this.isLoading = true;
    this.adminService.getAllAssessments().subscribe({
      next: (data) => {
        this.assessments = (data || []).sort((a, b) =>
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        );
        this.filteredAssessments = [...this.assessments];
        // Extract unique domains
        this.domains = [...new Set(this.assessments.map(a => a.domain).filter(Boolean))];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.error = err?.error?.message || 'Failed to load assessments.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  applyFilters(): void {
    this.filteredAssessments = this.assessments.filter(a => {
      const matchesDomain = !this.domainFilter || a.domain === this.domainFilter;
      const matchesGrade = !this.gradeFilter || a.grade === this.gradeFilter;
      return matchesDomain && matchesGrade;
    });
  }
}
