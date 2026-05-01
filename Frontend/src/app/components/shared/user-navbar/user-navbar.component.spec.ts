import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserNavbar } from './user-navbar.component';

describe('UserNavbar', () => {
  let component: UserNavbar;
  let fixture: ComponentFixture<UserNavbar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserNavbar],
    }).compileComponents();

    fixture = TestBed.createComponent(UserNavbar);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
