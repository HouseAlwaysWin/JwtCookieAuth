import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LineCallbackComponent } from './line-callback.component';

describe('LineCallbackComponent', () => {
  let component: LineCallbackComponent;
  let fixture: ComponentFixture<LineCallbackComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ LineCallbackComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(LineCallbackComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
