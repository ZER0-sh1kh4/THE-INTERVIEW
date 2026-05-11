export interface Notification {
  id: number;
  userId: number;
  title: string;
  message: string;
  actionUrl?: string;
  isRead: boolean;
  createdAt: string;
}
