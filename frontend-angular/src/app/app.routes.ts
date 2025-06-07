import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { GoogleSearchComponent } from './pages/google-search/google-search.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'google-search', component: GoogleSearchComponent },
];
