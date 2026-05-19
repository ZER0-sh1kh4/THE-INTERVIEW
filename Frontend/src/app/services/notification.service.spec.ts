import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { NotificationService } from './notification.service';
import { environment } from '../../environments/environment';

describe('NotificationService', () => {
  let service: NotificationService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiUrl}/auth/notifications`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [NotificationService, provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(NotificationService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('fetchNotifications()', () => {
    it('should GET and return normalized notifications', () => {
      service.fetchNotifications().subscribe(notifications => {
        expect(notifications.length).toBe(2);
        expect(notifications[0].title).toBe('Welcome');
      });
      const req = httpMock.expectOne(apiUrl);
      expect(req.request.method).toBe('GET');
      req.flush({ success: true, message: '', data: [
        { id: 1, userId: 1, title: 'Welcome', message: 'Hi', isRead: false, createdAt: '' },
        { id: 2, userId: 1, title: 'Update', message: 'Done', isRead: true, createdAt: '' }
      ]});
    });

    it('should update the unread count observable', () => {
      let unreadCount = 0;
      service.unreadCount$.subscribe(count => unreadCount = count);
      service.fetchNotifications().subscribe();
      httpMock.expectOne(apiUrl).flush({ success: true, message: '', data: [
        { id: 1, userId: 1, title: 'A', message: '', isRead: false, createdAt: '' },
        { id: 2, userId: 1, title: 'B', message: '', isRead: false, createdAt: '' },
        { id: 3, userId: 1, title: 'C', message: '', isRead: true, createdAt: '' }
      ]});
      expect(unreadCount).toBe(2);
    });
  });

  describe('markAllAsRead()', () => {
    it('should PUT to mark all as read and reset unread count', () => {
      service.fetchNotifications().subscribe();
      httpMock.expectOne(apiUrl).flush({ success: true, message: '', data: [
        { id: 1, userId: 1, title: 'N', message: '', isRead: false, createdAt: '' }
      ]});

      service.markAllAsRead().subscribe();
      const req = httpMock.expectOne(`${apiUrl}/read`);
      expect(req.request.method).toBe('PUT');
      req.flush({ success: true });

      let count = -1;
      service.unreadCount$.subscribe(c => count = c);
      expect(count).toBe(0);
    });
  });
});
