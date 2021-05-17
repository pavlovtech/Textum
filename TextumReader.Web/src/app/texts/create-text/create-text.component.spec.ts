import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CreateTextComponent } from './create-text.component';

describe('TextCreationComponent', () => {
  let component: CreateTextComponent;
  let fixture: ComponentFixture<CreateTextComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CreateTextComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CreateTextComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
