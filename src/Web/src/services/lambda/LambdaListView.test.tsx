import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { LambdaListView } from './LambdaListView';
import { createLambdaFunction, getLambdaFunctions } from '../../api/client';
import type { LambdaFunctionListResult } from '../../api/client';

vi.mock('../../api/client');

const getLambdaFunctionsMock = vi.mocked(getLambdaFunctions);
const createLambdaFunctionMock = vi.mocked(createLambdaFunction);

const listResult: LambdaFunctionListResult = {
  functions: [
    {
      functionName: 'process-orders',
      runtime: 'dotnet8',
      description: 'Order processor',
      lastModified: '2026-01-02T03:04:05Z',
      memorySize: 256,
      timeout: 30,
    },
    {
      functionName: 'resize-images',
      runtime: 'python3.12',
      description: 'Image resizer',
      lastModified: '2026-02-03T04:05:06Z',
      memorySize: 512,
      timeout: 60,
    },
  ],
};

function renderView() {
  return render(
    <MemoryRouter>
      <LambdaListView serviceKey="lambda" />
    </MemoryRouter>,
  );
}

describe('LambdaListView', () => {
  beforeEach(() => {
    getLambdaFunctionsMock.mockResolvedValue(listResult);
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows a loading state before functions arrive', () => {
    getLambdaFunctionsMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('lambda-list-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getLambdaFunctionsMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('lambda-list-error')).toBeInTheDocument());
  });

  it('renders a row per function with a link to the detail route', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('lambda-list-view')).toBeInTheDocument());

    expect(screen.getByTestId('data-list-row-process-orders')).toBeInTheDocument();
    expect(screen.getByTestId('data-list-row-resize-images')).toBeInTheDocument();

    const links = screen.getAllByTestId('lambda-list-link');
    expect(links).toHaveLength(2);
    expect(links[0]).toHaveAttribute('href', '/services/lambda/process-orders');
  });

  it('reloads functions when auto-refresh fires', async () => {
    vi.useFakeTimers();
    try {
      renderView();

      await act(async () => {
        await Promise.resolve();
      });
      expect(getLambdaFunctionsMock).toHaveBeenCalledTimes(1);

      fireEvent.click(screen.getByTestId('auto-refresh-switch'));
      await act(async () => {
        vi.advanceTimersByTime(5_000);
      });

      expect(getLambdaFunctionsMock).toHaveBeenCalledTimes(2);
    } finally {
      vi.useRealTimers();
    }
  });

  it('creates a function from the form and refreshes the list', async () => {
    createLambdaFunctionMock.mockResolvedValue();

    renderView();

    await waitFor(() => expect(screen.getByTestId('lambda-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('lambda-create-toggle'));

    fireEvent.change(screen.getByTestId('lambda-create-functionName'), {
      target: { value: 'new-fn' },
    });
    fireEvent.change(screen.getByTestId('lambda-create-runtime'), {
      target: { value: 'dotnet8' },
    });
    fireEvent.change(screen.getByTestId('lambda-create-handler'), {
      target: { value: 'index.handler' },
    });
    fireEvent.change(screen.getByTestId('lambda-create-role'), {
      target: { value: 'arn:aws:iam::000000000000:role/lambda' },
    });
    fireEvent.change(screen.getByTestId('lambda-create-description'), {
      target: { value: 'A new function' },
    });
    fireEvent.change(screen.getByTestId('lambda-create-memorySize'), {
      target: { value: '256' },
    });
    fireEvent.change(screen.getByTestId('lambda-create-timeout'), {
      target: { value: '15' },
    });
    fireEvent.change(screen.getByTestId('lambda-create-zipFileBase64'), {
      target: { value: 'QkFTRTY0' },
    });

    fireEvent.click(screen.getByTestId('lambda-create-submit'));

    await waitFor(() => expect(screen.getByTestId('lambda-create-status')).toBeInTheDocument());

    expect(createLambdaFunctionMock).toHaveBeenCalledWith({
      functionName: 'new-fn',
      runtime: 'dotnet8',
      handler: 'index.handler',
      role: 'arn:aws:iam::000000000000:role/lambda',
      description: 'A new function',
      memorySize: 256,
      timeout: 15,
      zipFileBase64: 'QkFTRTY0',
    });
    expect(getLambdaFunctionsMock).toHaveBeenCalledTimes(2);
    expect(screen.queryByTestId('lambda-create-form')).not.toBeInTheDocument();
  });

  it('hides the create form when the toggle is clicked twice', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('lambda-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('lambda-create-toggle'));
    expect(screen.getByTestId('lambda-create-form')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('lambda-create-toggle'));
    expect(screen.queryByTestId('lambda-create-form')).not.toBeInTheDocument();
  });

  it('shows an error when function creation fails', async () => {
    createLambdaFunctionMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('lambda-list-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('lambda-create-toggle'));
    fireEvent.click(screen.getByTestId('lambda-create-submit'));

    await waitFor(() => expect(screen.getByTestId('lambda-create-error')).toBeInTheDocument());
    expect(screen.getByTestId('lambda-create-form')).toBeInTheDocument();
  });
});
