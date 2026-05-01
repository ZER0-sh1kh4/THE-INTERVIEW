import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminNavbarComponent } from '../admin-navbar/admin-navbar.component';
import { AdminService } from '../../services/admin.service';
import { MCQQuestion } from '../../models/admin.models';

@Component({
  selector: 'app-admin-questions',
  standalone: true,
  imports: [CommonModule, FormsModule, AdminNavbarComponent],
  templateUrl: './admin-questions.component.html',
  styleUrl: './admin-questions.component.css'
})
export class AdminQuestionsComponent implements OnInit {
  questions: MCQQuestion[] = [];
  filteredQuestions: MCQQuestion[] = [];
  domains: string[] = [];
  domainFilter = '';
  isLoading = true;
  error = '';

  // Create / Edit drawer
  showModal = false;
  isEditing = false;
  isSaving = false;
  saveError = '';
  editId = 0;

  formDomain = '';
  formDomainCustom = '';
  formText = '';
  formOptionA = '';
  formOptionB = '';
  formOptionC = '';
  formOptionD = '';
  formCorrectOption = 'A';
  formSubtopic = '';

  // Delete dialog
  showDeleteDialog = false;
  deleteTarget: MCQQuestion | null = null;
  isDeleting = false;

  constructor(private adminService: AdminService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadQuestions();
  }

  loadQuestions(): void {
    this.isLoading = true;
    this.adminService.getAllQuestions().subscribe({
      next: (data) => {
        this.questions = data || [];
        this.domains = [...new Set(this.questions.map(q => q.domain).filter(Boolean))];
        this.applyFilters();
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.error = err?.error?.message || 'Failed to load questions.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  applyFilters(): void {
    this.filteredQuestions = this.questions.filter(q =>
      !this.domainFilter || q.domain === this.domainFilter
    );
  }

  /** True when the admin has chosen to create a brand-new domain. */
  get formDomainIsNew(): boolean {
    return this.formDomain === '__new';
  }

  openCreate(): void {
    this.resetForm();
    this.isEditing = false;
    this.showModal = true;
  }

  openEdit(q: MCQQuestion): void {
    this.isEditing = true;
    this.editId = q.id;
    // If the question's domain exists in our list, select it; otherwise treat as custom
    if (this.domains.includes(q.domain)) {
      this.formDomain = q.domain;
      this.formDomainCustom = '';
    } else {
      this.formDomain = '__new';
      this.formDomainCustom = q.domain;
    }
    this.formText = q.text;
    this.formOptionA = q.optionA;
    this.formOptionB = q.optionB;
    this.formOptionC = q.optionC;
    this.formOptionD = q.optionD;
    this.formCorrectOption = q.correctOption;
    this.formSubtopic = q.subtopic;
    this.saveError = '';
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.saveError = '';
  }

  saveQuestion(): void {
    const domain = this.formDomainIsNew ? this.formDomainCustom.trim() : this.formDomain.trim();

    if (!domain || !this.formText || !this.formOptionA || !this.formOptionB || !this.formOptionC || !this.formOptionD || !this.formSubtopic) {
      this.saveError = 'All fields are required.';
      return;
    }

    // Guard: if creating a new domain, double-check with the admin to prevent typos
    if (this.formDomainIsNew) {
      // Check for near-matches (case-insensitive) in existing domains
      const lowerDomain = domain.toLowerCase();
      const similar = this.domains.find(d => d.toLowerCase() === lowerDomain);
      if (similar) {
        this.saveError = `A domain called "${similar}" already exists. Please select it from the dropdown instead.`;
        return;
      }
      if (!confirm(`You are creating a new domain: "${domain}". This will be added to the database. Continue?`)) {
        return;
      }
    }

    this.isSaving = true;
    this.saveError = '';

    const payload: Partial<MCQQuestion> = {
      domain: domain,
      text: this.formText.trim(),
      optionA: this.formOptionA.trim(),
      optionB: this.formOptionB.trim(),
      optionC: this.formOptionC.trim(),
      optionD: this.formOptionD.trim(),
      correctOption: this.formCorrectOption,
      subtopic: this.formSubtopic.trim()
    };

    const req = this.isEditing
      ? this.adminService.updateQuestion(this.editId, payload)
      : this.adminService.createQuestion(payload);

    req.subscribe({
      next: () => {
        this.isSaving = false;
        this.showModal = false;
        this.loadQuestions();
      },
      error: (err) => {
        this.isSaving = false;
        this.saveError = err?.error?.message || 'Failed to save question.';
        this.cdr.detectChanges();
      }
    });
  }

  promptDelete(q: MCQQuestion): void {
    this.deleteTarget = q;
    this.showDeleteDialog = true;
  }

  cancelDelete(): void {
    this.deleteTarget = null;
    this.showDeleteDialog = false;
  }

  confirmDelete(): void {
    if (!this.deleteTarget) return;
    this.isDeleting = true;
    this.adminService.deleteQuestion(this.deleteTarget.id).subscribe({
      next: () => {
        this.isDeleting = false;
        this.showDeleteDialog = false;
        this.deleteTarget = null;
        this.loadQuestions();
      },
      error: () => {
        this.isDeleting = false;
        this.showDeleteDialog = false;
      }
    });
  }

  private resetForm(): void {
    this.editId = 0;
    this.formDomain = '';
    this.formDomainCustom = '';
    this.formText = '';
    this.formOptionA = '';
    this.formOptionB = '';
    this.formOptionC = '';
    this.formOptionD = '';
    this.formCorrectOption = 'A';
    this.formSubtopic = '';
    this.saveError = '';
  }
}
