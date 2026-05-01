import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

/**
 * Dashboard Component (Landing Page)
 * Purpose: This is the main entry point component for the application, representing the public landing/dashboard page.
 * It showcases the platform's features, methodology, and pricing, and provides login/register entry points.
 * 
 * Logic:
 * - Currently acts purely as a UI shell.
 * - In the future, actions here will link to the Authentication module or specific internal dashboards based on the user's role.
 */
@Component({
  selector: 'app-dashboard',
  imports: [RouterModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent {}
