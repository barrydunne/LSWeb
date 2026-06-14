import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { Route53ListView } from './Route53ListView';
import { createRoute53HostedZone, getRoute53HostedZones } from '../../api/client';
import type { Route53HostedZoneListResult } from '../../api/client';

vi.mock('../../api/client');

const getRoute53HostedZonesMock = vi.mocked(getRoute53HostedZones);
const createRoute53HostedZoneMock = vi.mocked(createRoute53HostedZone);

const result: Route53HostedZoneListResult = {
  hostedZones: [
    {
      id: '/hostedzone/Z123',
      name: 'example.com.',
      recordCount: 4,
      privateZone: false,
    },
    {
      id: '/hostedzone/Z456',
      name: 'internal.example.com.',
      recordCount: 2,
      privateZone: true,
    },
  ],
};

function renderView() {
  return render(
    <MemoryRouter>
      <Route53ListView serviceKey="route53" />
    </MemoryRouter>,
  );
}

describe('Route53ListView', () => {
  beforeEach(() => {
    getRoute53HostedZonesMock.mockResolvedValue(result);
    createRoute53HostedZoneMock.mockResolvedValue();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('shows a loading state before hosted zones arrive', () => {
    getRoute53HostedZonesMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('route53-list-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getRoute53HostedZonesMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('route53-list-error')).toBeInTheDocument(),
    );
  });

  it('renders a row per hosted zone', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('route53-list-view')).toBeInTheDocument(),
    );

    expect(screen.getByTestId('data-list-row-/hostedzone/Z123')).toBeInTheDocument();
    expect(screen.getByTestId('data-list-row-/hostedzone/Z456')).toBeInTheDocument();
  });

  it('shows the name, record count and public/private visibility', async () => {
    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('route53-list-view')).toBeInTheDocument(),
    );

    const names = screen.getAllByTestId('route53-list-link');
    const counts = screen.getAllByTestId('route53-list-record-count');
    const visibilities = screen.getAllByTestId('route53-list-visibility');
    expect(names[0]).toHaveTextContent('example.com.');
    expect(names[0]).toHaveAttribute('href', '/services/route53/%2Fhostedzone%2FZ123');
    expect(counts[0]).toHaveTextContent('4');
    expect(visibilities[0]).toHaveTextContent('Public');
    expect(visibilities[1]).toHaveTextContent('Private');
  });

  it('toggles the create form', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('route53-list-view')).toBeInTheDocument());

    expect(screen.queryByTestId('route53-create-form')).not.toBeInTheDocument();
    fireEvent.click(screen.getByTestId('route53-create-toggle'));
    expect(screen.getByTestId('route53-create-form')).toBeInTheDocument();
  });

  it('blocks creation when the name is not a fully qualified domain', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('route53-list-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('route53-create-toggle'));
    fireEvent.change(screen.getByTestId('route53-create-name'), { target: { value: 'example' } });
    fireEvent.click(screen.getByTestId('route53-create-submit'));

    expect(screen.getByTestId('route53-create-error')).toHaveTextContent('fully qualified');
    expect(createRoute53HostedZoneMock).not.toHaveBeenCalled();
  });

  it('creates a hosted zone with a comment', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('route53-list-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('route53-create-toggle'));
    fireEvent.change(screen.getByTestId('route53-create-name'), { target: { value: 'example.com' } });
    fireEvent.change(screen.getByTestId('route53-create-comment'), { target: { value: 'demo' } });
    fireEvent.click(screen.getByTestId('route53-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('route53-create-status')).toBeInTheDocument(),
    );
    expect(createRoute53HostedZoneMock).toHaveBeenCalledWith('example.com', 'demo');
  });

  it('creates a hosted zone with no comment', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('route53-list-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('route53-create-toggle'));
    fireEvent.change(screen.getByTestId('route53-create-name'), { target: { value: 'example.com' } });
    fireEvent.click(screen.getByTestId('route53-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('route53-create-status')).toBeInTheDocument(),
    );
    expect(createRoute53HostedZoneMock).toHaveBeenCalledWith('example.com', null);
  });

  it('shows an error when the create fails', async () => {
    createRoute53HostedZoneMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('route53-list-view')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('route53-create-toggle'));
    fireEvent.change(screen.getByTestId('route53-create-name'), { target: { value: 'example.com' } });
    fireEvent.click(screen.getByTestId('route53-create-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('route53-create-error')).toHaveTextContent(
        'Unable to create the hosted zone.',
      ),
    );
  });

  it('reloads the hosted zones when auto-refresh fires', async () => {
    vi.useFakeTimers();
    try {
      renderView();

      await vi.waitFor(() =>
        expect(screen.getByTestId('route53-list-view')).toBeInTheDocument(),
      );
      expect(getRoute53HostedZonesMock).toHaveBeenCalledTimes(1);

      fireEvent.click(screen.getByTestId('auto-refresh-switch'));
      act(() => {
        vi.advanceTimersByTime(5000);
      });

      await vi.waitFor(() => expect(getRoute53HostedZonesMock).toHaveBeenCalledTimes(2));
    } finally {
      vi.useRealTimers();
    }
  });
});
