import { afterEach, describe, expect, it, vi } from 'vitest';
import { cleanup, render, screen, waitFor } from '@testing-library/react';
import { LambdaCodeTab } from './LambdaCodeTab';
import { getLambdaFunctionCode } from '../../api/client';
import type { LambdaFunctionCodeResult } from '../../api/client';

vi.mock('../../api/client');

const getCodeMock = vi.mocked(getLambdaFunctionCode);

const zipCode: LambdaFunctionCodeResult = {
  functionName: 'process-orders',
  runtime: 'dotnet8',
  handler: 'Orders::Handler',
  packageType: 'Zip',
  codeSize: 2048,
  codeSha256: 'abc123=',
  repositoryType: 'S3',
  location: 'https://localstack/download.zip',
  imageUri: '',
};

function renderTab() {
  return render(<LambdaCodeTab functionName="process-orders" />);
}

describe('LambdaCodeTab', () => {
  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows a loading state before the code arrives', () => {
    getCodeMock.mockReturnValue(new Promise(() => {}));

    renderTab();

    expect(screen.getByTestId('lambda-code-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getCodeMock.mockRejectedValue(new Error('boom'));

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-code-error')).toBeInTheDocument());
  });

  it('renders the deployed package metadata and a download link for a zip package', async () => {
    getCodeMock.mockResolvedValue(zipCode);

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-code-tab')).toBeInTheDocument());
    expect(screen.getByTestId('lambda-code-handler')).toHaveTextContent('Orders::Handler');
    expect(screen.getByTestId('lambda-code-runtime')).toHaveTextContent('dotnet8');
    expect(screen.getByTestId('lambda-code-packageType')).toHaveTextContent('Zip');
    expect(screen.getByTestId('lambda-code-codeSize')).toHaveTextContent('2.0 KB');
    expect(screen.getByTestId('lambda-code-codeSha256')).toHaveTextContent('abc123=');
    expect(screen.getByTestId('lambda-code-repositoryType')).toHaveTextContent('S3');
    expect(screen.getByTestId('lambda-code-download')).toHaveAttribute(
      'href',
      'https://localstack/download.zip',
    );
  });

  it('renders the image URI as plain text and placeholders for absent fields', async () => {
    getCodeMock.mockResolvedValue({
      functionName: 'image-fn',
      runtime: '',
      handler: '',
      packageType: '',
      codeSize: 5 * 1024 * 1024,
      codeSha256: '',
      repositoryType: '',
      location: '000000000000.dkr.ecr.eu-west-1.amazonaws.com/app:latest',
      imageUri: '000000000000.dkr.ecr.eu-west-1.amazonaws.com/app:latest',
    });

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-code-tab')).toBeInTheDocument());
    expect(screen.getByTestId('lambda-code-handler')).toHaveTextContent('\u2014');
    expect(screen.getByTestId('lambda-code-runtime')).toHaveTextContent('\u2014');
    expect(screen.getByTestId('lambda-code-packageType')).toHaveTextContent('\u2014');
    expect(screen.getByTestId('lambda-code-codeSize')).toHaveTextContent('5.0 MB');
    expect(screen.getByTestId('lambda-code-location-value')).toHaveTextContent(
      '000000000000.dkr.ecr.eu-west-1.amazonaws.com/app:latest',
    );
    expect(screen.queryByTestId('lambda-code-download')).not.toBeInTheDocument();
  });

  it('renders a placeholder when no package location is reported', async () => {
    getCodeMock.mockResolvedValue({ ...zipCode, codeSize: 512, location: '' });

    renderTab();

    await waitFor(() => expect(screen.getByTestId('lambda-code-tab')).toBeInTheDocument());
    expect(screen.getByTestId('lambda-code-codeSize')).toHaveTextContent('512 B');
    expect(screen.getByTestId('lambda-code-location-empty')).toBeInTheDocument();
  });
});
