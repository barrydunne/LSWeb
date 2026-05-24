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
export const tailMethod = 'TailLogGroup';

export interface LiveLogEvent {
  timestamp: string;
  message: string;
}

export type LiveLogEventHandler = (event: LiveLogEvent) => void;

export interface LiveTailSubscription {
  stop: () => Promise<void>;
}

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

export async function streamLogGroup(
  logGroupName: string,
  filterPattern: string,
  handler: LiveLogEventHandler,
  connection: HubConnection = createConnection(),
): Promise<LiveTailSubscription> {
  await connection.start();
  const stream = connection.stream<LiveLogEvent>(tailMethod, logGroupName, filterPattern);
  const subscription = stream.subscribe({
    next: (event: LiveLogEvent) => handler(event),
    error: () => {},
    complete: () => {},
  });
  return {
    stop: async () => {
      subscription.dispose();
      await connection.stop();
    },
  };
}
