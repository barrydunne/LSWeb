import { afterEach, describe, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { EventBridgeBusesManager } from './EventBridgeBusesManager';
import {
  createEventBridgeEventBus,
  deleteEventBridgeEventBus,
  getEventBridgeEventBuses,
} from '../../api/client';

vi.mock('../../api/client');

const getEventBridgeEventBusesMock = vi.mocked(getEventBridgeEventBuses);
const createEventBridgeEventBusMock = vi.mocked(createEventBridgeEventBus);
const deleteEventBridgeEventBusMock = vi.mocked(deleteEventBridgeEventBus);

function renderManager() {
  render(
    <ThemeProvider colorMode="night">
      <EventBridgeBusesManager />
    </ThemeProvider>,
  );
}

describe('EventBridgeBusesManager', () => {
  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('lists buses and marks the default bus as non-deletable', async () => {
    getEventBridgeEventBusesMock.mockResolvedValue({
      buses: [
        { name: 'default', arn: 'arn:aws:events:eu-west-1:000000000000:event-bus/default' },
        { name: 'orders-bus', arn: 'arn:aws:events:eu-west-1:000000000000:event-bus/orders-bus' },
      ],
    });
    renderManager();

    await waitFor(() => expect(screen.getByTestId('eventbridge-bus-orders-bus')).toBeInTheDocument());
    expect(screen.getByTestId('eventbridge-bus-default-default')).toBeInTheDocument();
    expect(screen.getAllByTestId('confirm-trigger')).toHaveLength(1);
  });

  it('shows an empty message when there are no buses', async () => {
    getEventBridgeEventBusesMock.mockResolvedValue({ buses: [] });
    renderManager();

    await waitFor(() => expect(screen.getByTestId('eventbridge-buses-empty')).toBeInTheDocument());
  });

  it('shows an error when loading fails', async () => {
    getEventBridgeEventBusesMock.mockRejectedValue(new Error('boom'));
    renderManager();

    await waitFor(() => expect(screen.getByTestId('eventbridge-buses-error')).toBeInTheDocument());
  });

  it('requires a bus name before creating', async () => {
    getEventBridgeEventBusesMock.mockResolvedValue({ buses: [] });
    renderManager();

    await waitFor(() => expect(screen.getByTestId('eventbridge-buses-empty')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('eventbridge-bus-create'));

    expect(screen.getByTestId('eventbridge-buses-form-error')).toHaveTextContent('Enter an event bus name');
    expect(createEventBridgeEventBusMock).not.toHaveBeenCalled();
  });

  it('creates a bus and refreshes the list', async () => {
    getEventBridgeEventBusesMock.mockResolvedValue({ buses: [] });
    createEventBridgeEventBusMock.mockResolvedValue();
    renderManager();

    await waitFor(() => expect(screen.getByTestId('eventbridge-buses-empty')).toBeInTheDocument());
    const callsBefore = getEventBridgeEventBusesMock.mock.calls.length;

    fireEvent.change(screen.getByTestId('eventbridge-bus-name'), { target: { value: 'orders-bus' } });
    fireEvent.click(screen.getByTestId('eventbridge-bus-create'));

    await waitFor(() => expect(createEventBridgeEventBusMock).toHaveBeenCalledWith('orders-bus'));
    await waitFor(() =>
      expect(getEventBridgeEventBusesMock.mock.calls.length).toBeGreaterThan(callsBefore),
    );
  });

  it('shows an error when creating a bus fails', async () => {
    getEventBridgeEventBusesMock.mockResolvedValue({ buses: [] });
    createEventBridgeEventBusMock.mockRejectedValue(new Error('boom'));
    renderManager();

    await waitFor(() => expect(screen.getByTestId('eventbridge-buses-empty')).toBeInTheDocument());
    fireEvent.change(screen.getByTestId('eventbridge-bus-name'), { target: { value: 'orders-bus' } });
    fireEvent.click(screen.getByTestId('eventbridge-bus-create'));

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-buses-form-error')).toHaveTextContent('could not be created'),
    );
  });

  it('deletes a custom bus after confirmation', async () => {
    getEventBridgeEventBusesMock.mockResolvedValue({
      buses: [{ name: 'orders-bus', arn: 'arn:aws:events:eu-west-1:000000000000:event-bus/orders-bus' }],
    });
    deleteEventBridgeEventBusMock.mockResolvedValue();
    renderManager();

    await waitFor(() => expect(screen.getByTestId('eventbridge-bus-orders-bus')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(deleteEventBridgeEventBusMock).toHaveBeenCalledWith('orders-bus'));
  });

  it('shows an error when deleting a bus fails', async () => {
    getEventBridgeEventBusesMock.mockResolvedValue({
      buses: [{ name: 'orders-bus', arn: 'arn:aws:events:eu-west-1:000000000000:event-bus/orders-bus' }],
    });
    deleteEventBridgeEventBusMock.mockRejectedValue(new Error('boom'));
    renderManager();

    await waitFor(() => expect(screen.getByTestId('eventbridge-bus-orders-bus')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() =>
      expect(screen.getByTestId('eventbridge-buses-form-error')).toHaveTextContent('could not be deleted'),
    );
  });

  it('refreshes the list on demand', async () => {
    getEventBridgeEventBusesMock.mockResolvedValue({ buses: [] });
    renderManager();

    await waitFor(() => expect(screen.getByTestId('eventbridge-buses-empty')).toBeInTheDocument());
    const callsBefore = getEventBridgeEventBusesMock.mock.calls.length;

    fireEvent.click(screen.getByTestId('eventbridge-buses-refresh'));

    await waitFor(() =>
      expect(getEventBridgeEventBusesMock.mock.calls.length).toBeGreaterThan(callsBefore),
    );
  });
});
