import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { LambdaFunctionUrlTab } from './LambdaFunctionUrlTab';
import {
  createLambdaFunctionUrl,
  deleteLambdaFunctionUrl,
  getLambdaFunctionUrl,
  testLambdaFunctionUrl,
  updateLambdaFunctionUrl,
} from '../../api/client';
import type { LambdaFunctionUrlResult } from '../../api/client';

vi.mock('../../api/client');

const getUrlMock = vi.mocked(getLambdaFunctionUrl);
const createUrlMock = vi.mocked(createLambdaFunctionUrl);
const updateUrlMock = vi.mocked(updateLambdaFunctionUrl);
const deleteUrlMock = vi.mocked(deleteLambdaFunctionUrl);
const testUrlMock = vi.mocked(testLambdaFunctionUrl);

const configured: LambdaFunctionUrlResult = {
  configured: true,
  functionUrl: 'https://abc.lambda-url.eu-west-1.on.aws/',
  authType: 'NONE',
  creationTime: '2026-01-02T03:04:05Z',
  lastModifiedTime: '2026-01-03T03:04:05Z',
};

const notConfigured: LambdaFunctionUrlResult = {
  configured: false,
  functionUrl: '',
  authType: '',
  creationTime: '',
  lastModifiedTime: '',
};

function renderTab() {
  return render(<LambdaFunctionUrlTab functionName="process-orders" />);
}

describe('LambdaFunctionUrlTab', () => {
  beforeEach(() => {
    createUrlMock.mockResolvedValue();
    updateUrlMock.mockResolvedValue();
    deleteUrlMock.mockResolvedValue();
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows a loading state before the configuration arrives', () => {
    getUrlMock.mockReturnValue(new Promise(() => {}));

    renderTab();

    expect(screen.getByTestId('lambda-url-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getUrlMock.mockRejectedValue(new Error('boom'));

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-url-error')).toBeInTheDocument());
  });

  it('renders the configured URL with a link and current auth mode', async () => {
    getUrlMock.mockResolvedValue(configured);

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-url-tab')).toBeInTheDocument());
    expect(screen.getByTestId('lambda-url-link')).toHaveAttribute(
      'href',
      'https://abc.lambda-url.eu-west-1.on.aws/',
    );
    expect(screen.getByTestId('lambda-url-current-auth')).toHaveTextContent('NONE');
    expect(screen.getByTestId('lambda-url-auth-type')).toHaveValue('NONE');
  });

  it('creates a function URL when none is configured', async () => {
    getUrlMock.mockResolvedValueOnce(notConfigured).mockResolvedValueOnce(configured);

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-url-empty')).toBeInTheDocument());
    fireEvent.change(screen.getByTestId('lambda-url-auth-type'), { target: { value: 'AWS_IAM' } });
    fireEvent.click(screen.getByTestId('lambda-url-create'));

    await waitFor(() => expect(screen.getByTestId('lambda-url-tab')).toBeInTheDocument());
    expect(createUrlMock).toHaveBeenCalledWith('process-orders', 'AWS_IAM');
  });

  it('shows a mutation error when the create fails', async () => {
    getUrlMock.mockResolvedValue(notConfigured);
    createUrlMock.mockRejectedValue(new Error('boom'));

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-url-empty')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('lambda-url-create'));

    await waitFor(() => expect(screen.getByTestId('lambda-url-mutation-error')).toBeInTheDocument());
  });

  it('updates the auth mode of an existing URL', async () => {
    getUrlMock.mockResolvedValue(configured);

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-url-tab')).toBeInTheDocument());
    fireEvent.change(screen.getByTestId('lambda-url-auth-type'), { target: { value: 'AWS_IAM' } });
    fireEvent.click(screen.getByTestId('lambda-url-update'));

    await waitFor(() => expect(updateUrlMock).toHaveBeenCalledWith('process-orders', 'AWS_IAM'));
  });

  it('deletes the URL after confirmation', async () => {
    getUrlMock.mockResolvedValueOnce(configured).mockResolvedValueOnce(notConfigured);

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-url-tab')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('confirm-trigger'));
    fireEvent.click(screen.getByTestId('confirm-accept'));

    await waitFor(() => expect(screen.getByTestId('lambda-url-empty')).toBeInTheDocument());
    expect(deleteUrlMock).toHaveBeenCalledWith('process-orders');
  });

  it('runs a test request and shows the status and body', async () => {
    getUrlMock.mockResolvedValue(configured);
    testUrlMock.mockResolvedValue({ statusCode: 200, body: '{"ok":true}' });

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-url-tab')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('lambda-url-test'));

    await waitFor(() => expect(screen.getByTestId('lambda-url-test-result')).toBeInTheDocument());
    expect(screen.getByTestId('lambda-url-test-result')).toHaveTextContent('Status 200');
    expect(screen.getByTestId('lambda-url-test-result')).toHaveTextContent('{"ok":true}');
  });

  it('shows an error when the test request fails', async () => {
    getUrlMock.mockResolvedValue(configured);
    testUrlMock.mockRejectedValue(new Error('boom'));

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-url-tab')).toBeInTheDocument());
    fireEvent.click(screen.getByTestId('lambda-url-test'));

    await waitFor(() => expect(screen.getByTestId('lambda-url-test-error')).toBeInTheDocument());
  });
});
