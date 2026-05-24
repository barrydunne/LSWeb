import { afterEach, describe, expect, it, vi } from 'vitest';
import { HubConnectionBuilder, type HubConnection } from '@microsoft/signalr';
import {
  createConnection,
  notificationMethod,
  streamHubUrl,
  streamLogGroup,
  subscribeToNotifications,
  tailMethod,
  type LiveLogEvent,
  type Notification,
} from './notifications';

vi.mock('@microsoft/signalr', () => {
  const built: { connection: Partial<HubConnection> } = { connection: {} };
  const build = vi.fn(() => built.connection);
  const withAutomaticReconnect = vi.fn(() => ({ build }));
  const withUrl = vi.fn(() => ({ withAutomaticReconnect }));
  const HubConnectionBuilder = vi.fn(() => ({ withUrl }));
  return { HubConnectionBuilder, __built: built };
});

const builderModule = (await import('@microsoft/signalr')) as unknown as {
  HubConnectionBuilder: ReturnType<typeof vi.fn>;
  __built: { connection: Partial<HubConnection> };
};

function fakeConnection() {
  return {
    on: vi.fn(),
    start: vi.fn().mockResolvedValue(undefined),
    stop: vi.fn().mockResolvedValue(undefined),
  } as unknown as HubConnection & {
    on: ReturnType<typeof vi.fn>;
    start: ReturnType<typeof vi.fn>;
    stop: ReturnType<typeof vi.fn>;
  };
}

const sample: Notification = {
  operationId: 'op-1',
  operation: 'catalogue-refresh',
  state: 'Succeeded',
  message: 'Done.',
  occurredAt: '2026-01-02T03:04:05Z',
};

describe('notifications', () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it('createConnection builds a reconnecting connection against the stream hub', () => {
    const connection = fakeConnection();
    builderModule.__built.connection = connection;

    const result = createConnection();

    expect(HubConnectionBuilder).toHaveBeenCalledTimes(1);
    expect(result).toBe(connection);
  });

  it('subscribeToNotifications registers the handler and starts the provided connection', async () => {
    const connection = fakeConnection();
    const handler = vi.fn();

    const subscription = await subscribeToNotifications(handler, connection);

    expect(connection.on).toHaveBeenCalledWith(notificationMethod, expect.any(Function));
    expect(connection.start).toHaveBeenCalledTimes(1);

    const registered = connection.on.mock.calls[0][1] as (notification: Notification) => void;
    registered(sample);
    expect(handler).toHaveBeenCalledWith(sample);

    await subscription.stop();
    expect(connection.stop).toHaveBeenCalledTimes(1);
  });

  it('subscribeToNotifications creates a connection when none is supplied', async () => {
    const connection = fakeConnection();
    builderModule.__built.connection = connection;
    const handler = vi.fn();

    await subscribeToNotifications(handler);

    expect(builderModule.HubConnectionBuilder).toHaveBeenCalledTimes(1);
    expect(connection.start).toHaveBeenCalledTimes(1);
  });

  it('exposes the stream hub url constant', () => {
    expect(streamHubUrl).toBe('/hub/stream');
  });

  it('streamLogGroup starts the connection, subscribes and forwards events', async () => {
    let observer: {
      next: (event: LiveLogEvent) => void;
      error: (error: unknown) => void;
      complete: () => void;
    } | null = null;
    const dispose = vi.fn();
    const stream = vi.fn(() => ({
      subscribe: (incoming: typeof observer) => {
        observer = incoming;
        return { dispose };
      },
    }));
    const connection = {
      start: vi.fn().mockResolvedValue(undefined),
      stop: vi.fn().mockResolvedValue(undefined),
      stream,
    } as unknown as HubConnection;
    const handler = vi.fn();

    const subscription = await streamLogGroup('/aws/lambda/orders', 'ERROR', handler, connection);

    expect(connection.start).toHaveBeenCalledTimes(1);
    expect(stream).toHaveBeenCalledWith(tailMethod, '/aws/lambda/orders', 'ERROR');

    const event: LiveLogEvent = { timestamp: '2026-01-01T00:00:00Z', message: 'live' };
    observer!.next(event);
    expect(handler).toHaveBeenCalledWith(event);
    observer!.error(new Error('ignored'));
    observer!.complete();

    await subscription.stop();
    expect(dispose).toHaveBeenCalledTimes(1);
    expect((connection.stop as ReturnType<typeof vi.fn>)).toHaveBeenCalledTimes(1);
  });

  it('streamLogGroup creates a connection when none is supplied', async () => {
    const dispose = vi.fn();
    const connection = {
      start: vi.fn().mockResolvedValue(undefined),
      stop: vi.fn().mockResolvedValue(undefined),
      stream: vi.fn(() => ({ subscribe: () => ({ dispose }) })),
    } as unknown as HubConnection;
    builderModule.__built.connection = connection;

    await streamLogGroup('/aws/lambda/orders', '', vi.fn());

    expect(builderModule.HubConnectionBuilder).toHaveBeenCalledTimes(1);
    expect((connection.start as ReturnType<typeof vi.fn>)).toHaveBeenCalledTimes(1);
  });
});
