import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { CopyAsCliButton } from './CopyAsCliButton';
import { getCliSnippet } from '../api/client';

vi.mock('../api/client');

const writeText = vi.fn<(text: string) => Promise<void>>();

beforeEach(() => {
  vi.mocked(getCliSnippet).mockReset();
  writeText.mockReset();
  writeText.mockResolvedValue(undefined);
  Object.defineProperty(navigator, 'clipboard', {
    configurable: true,
    value: { writeText },
  });
});

afterEach(() => {
  vi.restoreAllMocks();
});

describe('CopyAsCliButton', () => {
  it('renders the hint and default label before generating', () => {
    render(<CopyAsCliButton service="s3api" operation="list-buckets" />);

    expect(screen.getByTestId('copy-as-cli-title')).toBeInTheDocument();
    expect(screen.getByTestId('copy-as-cli-button')).toHaveTextContent('Copy as CLI');
    expect(screen.getByTestId('copy-as-cli-hint')).toBeInTheDocument();
    expect(screen.queryByTestId('copy-as-cli-command')).not.toBeInTheDocument();
  });

  it('fetches the snippet, renders the command and copies it to the clipboard', async () => {
    vi.mocked(getCliSnippet).mockResolvedValue({
      command: 'aws s3api list-buckets --endpoint-url http://localhost:4566 --region eu-west-1',
    });
    render(<CopyAsCliButton service="s3api" operation="list-buckets" />);

    await userEvent.click(screen.getByTestId('copy-as-cli-button'));

    await waitFor(() => {
      expect(screen.getByTestId('copy-as-cli-command')).toHaveTextContent(
        'aws s3api list-buckets --endpoint-url http://localhost:4566 --region eu-west-1',
      );
    });
    expect(writeText).toHaveBeenCalledWith(
      'aws s3api list-buckets --endpoint-url http://localhost:4566 --region eu-west-1',
    );
    expect(screen.getByTestId('copy-as-cli-button')).toHaveTextContent('Copied');
  });

  it('forwards the provided parameters and label to the request', async () => {
    vi.mocked(getCliSnippet).mockResolvedValue({ command: 'aws s3api head-bucket --bucket my-bucket' });
    render(
      <CopyAsCliButton
        service="s3api"
        operation="head-bucket"
        parameters={[{ name: 'bucket', value: 'my-bucket', isSensitive: false }]}
        label="Copy command"
      />,
    );

    expect(screen.getByTestId('copy-as-cli-button')).toHaveTextContent('Copy command');

    await userEvent.click(screen.getByTestId('copy-as-cli-button'));

    await waitFor(() => {
      expect(screen.getByTestId('copy-as-cli-command')).toBeInTheDocument();
    });
    expect(getCliSnippet).toHaveBeenCalledWith({
      service: 's3api',
      operation: 'head-bucket',
      parameters: [{ name: 'bucket', value: 'my-bucket', isSensitive: false }],
    });
  });

  it('shows an error when snippet generation fails', async () => {
    vi.mocked(getCliSnippet).mockRejectedValue(new Error('boom'));
    render(<CopyAsCliButton service="s3api" operation="list-buckets" />);

    await userEvent.click(screen.getByTestId('copy-as-cli-button'));

    await waitFor(() => {
      expect(screen.getByTestId('copy-as-cli-error')).toBeInTheDocument();
    });
    expect(screen.queryByTestId('copy-as-cli-command')).not.toBeInTheDocument();
    expect(writeText).not.toHaveBeenCalled();
  });

  it('reports an error when the clipboard write fails after generating', async () => {
    vi.mocked(getCliSnippet).mockResolvedValue({ command: 'aws s3api list-buckets' });
    writeText.mockRejectedValue(new Error('denied'));
    render(<CopyAsCliButton service="s3api" operation="list-buckets" />);

    await userEvent.click(screen.getByTestId('copy-as-cli-button'));

    await waitFor(() => {
      expect(screen.getByTestId('copy-as-cli-error')).toBeInTheDocument();
    });
    expect(screen.getByTestId('copy-as-cli-button')).not.toHaveTextContent('Copied');
  });
});
