import { Component, inject } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Router, RouterLink } from '@angular/router';
import { MatMenuModule } from '@angular/material/menu';
import { CommonModule } from '@angular/common';
import {
  MatSnackBar,
  MatSnackBarLabel,
  MatSnackBarModule,
} from '@angular/material/snack-bar';
@Component({
  selector: 'app-navbar',
  imports: [
    MatToolbarModule,
    RouterLink,
    MatMenuModule,
    MatSnackBarModule,
    MatButtonModule,
    MatIconModule,
    CommonModule,
  ],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css',
})
export class NavbarComponent {
  matSnackBar = inject(MatSnackBar);

  router = inject(Router);
}
