import { TestBed, inject } from '@angular/core/testing';
import {
  HttpClientTestingModule,
  HttpTestingController,
} from '@angular/common/http/testing';
import { HTTP_INTERCEPTORS, HttpClient } from '@angular/common/http';

import { errorInterceptor } from './error.interceptor';

describe('errorInterceptor', () => {
  let httpClient: HttpClient;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        {
          provide: HTTP_INTERCEPTORS,
          useValue: errorInterceptor,
          multi: true,
        },
      ],
    });

    httpClient = TestBed.inject(HttpClient);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  it('should be created', () => {
    expect(errorInterceptor).toBeTruthy();
  });

  it('should handle an HTTP error', (done) => {
    httpClient.get('/test').subscribe({
      next: () => fail('should have failed with the error'),
      error: (error) => {
        expect(error.message).toContain('Error Code: 404');
        done();
      },
    });

    const req = httpTestingController.expectOne('/test');
    req.flush('test error', { status: 404, statusText: 'Not Found' });
  });

  afterEach(() => {
    httpTestingController.verify();
  });
});
