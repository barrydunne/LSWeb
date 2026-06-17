import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { cleanup, render, screen, waitFor } from '@testing-library/react';
import { CircuitBreakerBanner } from './CircuitBreakerBanner';
import { getCircuitStatus } from '../api/client';

vi.mock('../api/client');

const getCircuitStatusMock = vi.mocked(getCircuitStatus);

describe('CircuitBreakerBanner', () => {
  beforeEach(() => {
    getCircuitStatusMock.mockResolvedValue({ isOpen: false, affectedServices: [] });
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
    vi.useRealTimers();
  });

  it('renders nothing while the circuit is closed', async () => {
    render(<CircuitBreakerBanner />);

    await waitFor(() => expect(getCircuitStatusMock).toHaveBeenCalled());
    expect(screen.queryByTestId('circuit-breaker-banner')).not.toBeInTheDocument();
  });

  it('renders nothing before the first status resolves', () => {
    getCircuitStatusMock.mockReturnValue(new Promise(() => {}));

    render(<CircuitBreakerBanner />);

    expect(screen.queryByTestId('circuit-breaker-banner')).not.toBeInTheDocument();
  });

  it('shows a banner naming a single affected service with recovery guidance', async () => {
    getCircuitStatusMock.mockResolvedValue({ isOpen: true, affectedServices: ['s3'] });

    render(<CircuitBreakerBanner />);

    await waitFor(() => expect(screen.getByTestId('circuit-breaker-banner')).toBeInTheDocument());
    expect(screen.getByTestId('circuit-breaker-banner-title')).toHaveTextContent(
      'Connection to s3 temporarily suspended',
    );
    const detail = screen.getByTestId('circuit-breaker-banner-detail');
    expect(detail).toHaveTextContent('Try restarting the application container and reloading');
    expect(detail).not.toHaveTextContent('LocalStackWeb');
    expect(detail).not.toHaveTextContent('docker restart');
    expect(detail).toHaveTextContent('calls to s3 is being rejected');
  });

  it('joins two affected services with "and"', async () => {
    getCircuitStatusMock.mockResolvedValue({ isOpen: true, affectedServices: ['s3', 'sqs'] });

    render(<CircuitBreakerBanner />);

    await waitFor(() => expect(screen.getByTestId('circuit-breaker-banner')).toBeInTheDocument());
    expect(screen.getByTestId('circuit-breaker-banner-title')).toHaveTextContent(
      'Connection to s3 and sqs temporarily suspended',
    );
    expect(screen.getByTestId('circuit-breaker-banner-detail')).toHaveTextContent(
      'calls to s3 and sqs are being rejected',
    );
  });

  it('joins three or more affected services with commas and a trailing "and"', async () => {
    getCircuitStatusMock.mockResolvedValue({
      isOpen: true,
      affectedServices: ['cognito', 's3', 'sqs'],
    });

    render(<CircuitBreakerBanner />);

    await waitFor(() => expect(screen.getByTestId('circuit-breaker-banner')).toBeInTheDocument());
    expect(screen.getByTestId('circuit-breaker-banner-title')).toHaveTextContent(
      'Connection to cognito, s3 and sqs temporarily suspended',
    );
  });

  it('falls back to a generic label when the open breaker lists no services', async () => {
    getCircuitStatusMock.mockResolvedValue({ isOpen: true, affectedServices: [] });

    render(<CircuitBreakerBanner />);

    await waitFor(() => expect(screen.getByTestId('circuit-breaker-banner')).toBeInTheDocument());
    expect(screen.getByTestId('circuit-breaker-banner-title')).toHaveTextContent(
      'Connection to a backend service temporarily suspended',
    );
  });

  it('clears the banner automatically once the breaker closes on a later poll', async () => {
    vi.useFakeTimers();
    getCircuitStatusMock
      .mockResolvedValueOnce({ isOpen: true, affectedServices: ['s3'] })
      .mockResolvedValue({ isOpen: false, affectedServices: [] });

    render(<CircuitBreakerBanner />);

    await vi.waitFor(() => expect(screen.getByTestId('circuit-breaker-banner')).toBeInTheDocument());

    await vi.advanceTimersByTimeAsync(8000);

    await vi.waitFor(() =>
      expect(screen.queryByTestId('circuit-breaker-banner')).not.toBeInTheDocument(),
    );
  });

  it('keeps the last known state when a poll fails', async () => {
    getCircuitStatusMock.mockRejectedValue(new Error('network'));

    render(<CircuitBreakerBanner />);

    await waitFor(() => expect(getCircuitStatusMock).toHaveBeenCalled());
    expect(screen.queryByTestId('circuit-breaker-banner')).not.toBeInTheDocument();
  });
});
