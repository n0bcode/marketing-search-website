import { AppComponent } from './app.component';
import { TestBed, ComponentFixture } from '@angular/core/testing'; // Import ComponentFixture

describe('AppComponent', () => {
  let fixture: ComponentFixture<AppComponent>; // Khai báo fixture
  let component: AppComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [AppComponent], // Khai báo AppComponent trong declarations
      imports: [], // Loại bỏ AppComponent khỏi imports (trong TestBed)
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
    component = fixture.componentInstance;
  });

  it('should create the app', () => {
    expect(component).toBeTruthy();
  });

  it(`should have the 'angular-frontend' title`, () => {
    expect(component.title).toEqual('angular-frontend');
  });

  it('should render title', () => {
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain(
      'Hello, angular-frontend'
    );
  });
});
