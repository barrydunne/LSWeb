import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { DiagnosticsPage } from './DiagnosticsPage';
import { getDiagnostics } from '../api/client';
import type { DiagnosticsResult } from '../api/client';

vi.mock('../api/client');

const getDiagnosticsMock = vi.mocked(getDiagnostics);

const maskedResult: DiagnosticsResult = {
  configuration: [
    { name: 'Access key', value: '********', source: 'EnvironmentVariable', isSensitive: true },
    { name: 'Region', value: 'eu-west-1', source: 'Default', isSensitive: false },
  ],
  endpoint: 'http://localhost:4566',
  region: 'eu-west-1',
  connectivityStatus: 'Connected',
  connectivityError: null,
  revealAllowed: true,
};

describe('DiagnosticsPage', () => {
  beforeEach(() => {
    getDiagnosticsMock.mockResolvedValue(maskedResult);
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('shows a loading state before diagnostics arrive', () => {
    getDiagnosticsMock.mockReturnValue(new Promise(() => {}));

    render(<DiagnosticsPage />);

    expect(screen.getByTestId('diagnostics-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getDiagnosticsMock.mockRejectedValue(new Error('boom'));

    render(<DiagnosticsPage />);

    await waitFor(() => expect(screen.getByTestId('diagnostics-error')).toBeInTheDocument());
  });

  it('renders the summary and masked configuration', async () => {
    render(<DiagnosticsPage />);

    await waitFor(() => expect(screen.getByTestId('diagnostics-summary')).toBeInTheDocument());

    expect(screen.getByTestId('diagnostics-status')).toHaveTextContent('Connected');
    expect(screen.getByTestId('diagnostics-endpoint')).toHaveTextContent('http://localhost:4566');
    expect(screen.getByTestId('diagnostics-region')).toHaveTextContent('eu-west-1');
    expect(screen.getAllByTestId('masked-value-field')).toHaveLength(2);
    expect(screen.queryByTestId('diagnostics-status-error')).not.toBeInTheDocument();
    expect(screen.queryByTestId('diagnostics-reveal-disabled')).not.toBeInTheDocument();
  });

  it('re-requests revealed values when the toggle is clicked', async () => {
    render(<DiagnosticsPage />);

    await waitFor(() => expect(screen.getByTestId('diagnostics-summary')).toBeInTheDocument());
    expect(getDiagnosticsMock).toHaveBeenLastCalledWith(false, expect.anything());

    getDiagnosticsMock.mockResolvedValue({
      ...maskedResult,
      configuration: [
        { name: 'Access key', value: 'AKIAEXAMPLE', source: 'EnvironmentVariable', isSensitive: true },
        { name: 'Region', value: 'eu-west-1', source: 'Default', isSensitive: false },
      ],
    });

    await userEvent.click(screen.getByTestId('masked-value-toggle'));

    await waitFor(() => expect(getDiagnosticsMock).toHaveBeenLastCalledWith(true, expect.anything()));
    await waitFor(() => expect(screen.getByText('AKIAEXAMPLE')).toBeInTheDocument());
  });

  it('shows the reveal-disabled hint and connectivity error when applicable', async () => {
    getDiagnosticsMock.mockResolvedValue({
      ...maskedResult,
      connectivityStatus: 'Unreachable',
      connectivityError: 'Connection refused',
      revealAllowed: false,
    });

    render(<DiagnosticsPage />);

    await waitFor(() => expect(screen.getByTestId('diagnostics-reveal-disabled')).toBeInTheDocument());
    expect(screen.getByTestId('diagnostics-status-error')).toHaveTextContent('Connection refused');
    expect(screen.queryByTestId('masked-value-toggle')).not.toBeInTheDocument();
  });
});
