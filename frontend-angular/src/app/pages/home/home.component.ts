import { Component, inject } from '@angular/core';
import { NgModule } from '@angular/core';
import { RouterLink, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
@Component({
  selector: 'app-home',
  imports: [RouterLink, CommonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export class HomeComponent {}
