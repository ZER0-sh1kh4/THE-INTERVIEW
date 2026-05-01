import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { UserNavbarComponent } from '../../shared/user-navbar/user-navbar.component';

@Component({
  selector: 'app-assessment-domain',
  standalone: true,
  imports: [CommonModule, UserNavbarComponent],
  templateUrl: './assessment-domain.component.html',
  styleUrls: ['./assessment-domain.component.css']
})
export class AssessmentDomainComponent {
  domains = [
    { name: 'C#', icon: 'terminal', desc: 'Enterprise-grade systems and backend architecture.' },
    { name: 'Python', icon: 'query_stats', desc: 'Data science, machine learning, and automation.' },
    { name: 'React', icon: 'layers', desc: 'Modern frontend ecosystems and component-driven UI.' },
    { name: 'Java', icon: 'coffee', desc: 'Robust enterprise infrastructure.' },
    { name: 'Rust', icon: 'memory', desc: 'Memory-safe systems programming.' },
    { name: 'AI / ML', icon: 'neurology', desc: 'Prompt engineering, LLM fine-tuning, and algorithms.' }
  ];

  constructor(private router: Router) {}

  selectDomain(domain: string) {
    this.router.navigate(['/assessments/start'], { queryParams: { domain } });
  }
}
