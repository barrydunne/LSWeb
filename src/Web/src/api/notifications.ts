import { HubConnectionBuilder, type HubConnection } from '@microsoft/signalr';

export type OperationState = 'InProgress' | 'Succeeded' | 'Failed';

export interface Notification {
  operationId: string;
  operation: string;
  state: OperationState;
  message: string;
  occurredAt: string;
}

export type NotificationHandler = (notification: Notification) => void;

export interface NotificationSubscription {
  stop: () => Promise<void>;
}

export const notificationMethod = 'notification';
export const streamHubUrl = '/hub/stream';

export function createConnection(): HubConnection {
  return new HubConnectionBuilder().withUrl(streamHubUrl).withAutomaticReconnect().build();
}

export async function subscribeToNotifications(
  handler: NotificationHandler,
  connection: HubConnection = createConnection(),
): Promise<NotificationSubscription> {
  connection.on(notificationMethod, (notification: Notification) => handler(notification));
  await connection.start();
  return { stop: () => connection.stop() };
}
