import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TextCreationComponent } from './text-creation.component';

describe('TextCreationComponent', () => {
  let component: TextCreationComponent;
  let fixture: ComponentFixture<TextCreationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TextCreationComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TextCreationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
