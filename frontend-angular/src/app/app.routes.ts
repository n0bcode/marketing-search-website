import { Routes } from '@angular/router';
import { TestsComponent } from './pages/tests/tests.component';
import { HomeComponent } from './pages/home/home.component';
import { GoogleSearchComponent } from './pages/google-search/google-search.component';
import { TwitterSearchComponent } from './pages/twitter-search/twitter-search.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'test', component: TestsComponent },
  { path: 'google-search', component: GoogleSearchComponent },
  { path: 'twitter-search', component: TwitterSearchComponent },
];
