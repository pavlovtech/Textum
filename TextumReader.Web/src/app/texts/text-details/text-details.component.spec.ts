import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TextDetailsComponent } from './text-details.component';

describe('TextCreationComponent', () => {
  let component: TextDetailsComponent;
  let fixture: ComponentFixture<TextDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TextDetailsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TextDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
