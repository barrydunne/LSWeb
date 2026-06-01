import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { CloudFormationExportsPanel } from './CloudFormationExportsPanel';
import { getExports, getImports } from '../../api/client';
import type {
  CloudFormationExportListResult,
  CloudFormationImportListResult,
} from '../../api/client';

vi.mock('../../api/client');

const getExportsMock = vi.mocked(getExports);
const getImportsMock = vi.mocked(getImports);

const exportsResult: CloudFormationExportListResult = {
  exports: [
    {
      name: 'shared-vpc-id',
      value: 'vpc-12345',
      exportingStackId:
        'arn:aws:cloudformation:eu-west-1:000000000000:stack/network-stack/abc-123',
    },
    {
      name: 'raw-export',
      value: 'raw-value',
      exportingStackId: 'not-an-arn',
    },
  ],
};

const importsResult: CloudFormationImportListResult = {
  importingStackNames: ['orders-stack', 'billing-stack'],
};

function renderPanel() {
  return render(
    <MemoryRouter>
      <CloudFormationExportsPanel />
    </MemoryRouter>,
  );
}

describe('CloudFormationExportsPanel', () => {
  beforeEach(() => {
    getExportsMock.mockResolvedValue(exportsResult);
    getImportsMock.mockResolvedValue(importsResult);
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('shows a loading message before exports resolve', () => {
    getExportsMock.mockReturnValue(new Promise(() => {}));
    renderPanel();

    expect(screen.getByTestId('cloudformation-exports-loading')).toBeInTheDocument();
  });

  it('renders exports with the exporting stack linked from its ARN', async () => {
    renderPanel();

    await waitFor(() => {
      expect(screen.getByTestId('cloudformation-exports-table')).toBeInTheDocument();
    });
    expect(screen.getByText('shared-vpc-id')).toBeInTheDocument();
    expect(screen.getByText('vpc-12345')).toBeInTheDocument();
    const stackLink = screen.getByRole('link', { name: 'network-stack' });
    expect(stackLink).toHaveAttribute('href', '/services/cloudformation/network-stack');
  });

  it('renders the raw exporting stack id when it is not an ARN', async () => {
    renderPanel();

    await waitFor(() => {
      expect(screen.getByText('not-an-arn')).toBeInTheDocument();
    });
    expect(screen.queryByRole('link', { name: 'not-an-arn' })).toBeNull();
  });

  it('shows an empty message when there are no exports', async () => {
    getExportsMock.mockResolvedValue({ exports: [] });
    renderPanel();

    await waitFor(() => {
      expect(screen.getByTestId('cloudformation-exports-empty')).toBeInTheDocument();
    });
  });

  it('shows an error message when loading exports fails', async () => {
    getExportsMock.mockRejectedValue(new Error('boom'));
    renderPanel();

    await waitFor(() => {
      expect(screen.getByTestId('cloudformation-exports-error')).toBeInTheDocument();
    });
  });

  it('lists importing stacks with links when an export is selected', async () => {
    renderPanel();

    await waitFor(() => {
      expect(screen.getByTestId('cloudformation-exports-table')).toBeInTheDocument();
    });
    fireEvent.click(screen.getByTestId('cloudformation-export-imports-shared-vpc-id'));

    await waitFor(() => {
      expect(screen.getByTestId('cloudformation-imports-list')).toBeInTheDocument();
    });
    expect(getImportsMock).toHaveBeenCalledWith('shared-vpc-id');
    const ordersLink = screen.getByRole('link', { name: 'orders-stack' });
    expect(ordersLink).toHaveAttribute('href', '/services/cloudformation/orders-stack');
    const billingLink = screen.getByRole('link', { name: 'billing-stack' });
    expect(billingLink).toHaveAttribute('href', '/services/cloudformation/billing-stack');
  });

  it('shows a loading message while importing stacks resolve', async () => {
    getImportsMock.mockReturnValue(new Promise(() => {}));
    renderPanel();

    await waitFor(() => {
      expect(screen.getByTestId('cloudformation-exports-table')).toBeInTheDocument();
    });
    fireEvent.click(screen.getByTestId('cloudformation-export-imports-shared-vpc-id'));

    expect(screen.getByTestId('cloudformation-imports-loading')).toBeInTheDocument();
  });

  it('shows an empty message when no stacks import the export', async () => {
    getImportsMock.mockResolvedValue({ importingStackNames: [] });
    renderPanel();

    await waitFor(() => {
      expect(screen.getByTestId('cloudformation-exports-table')).toBeInTheDocument();
    });
    fireEvent.click(screen.getByTestId('cloudformation-export-imports-shared-vpc-id'));

    await waitFor(() => {
      expect(screen.getByTestId('cloudformation-imports-empty')).toBeInTheDocument();
    });
  });

  it('shows an error message when loading importing stacks fails', async () => {
    getImportsMock.mockRejectedValue(new Error('boom'));
    renderPanel();

    await waitFor(() => {
      expect(screen.getByTestId('cloudformation-exports-table')).toBeInTheDocument();
    });
    fireEvent.click(screen.getByTestId('cloudformation-export-imports-shared-vpc-id'));

    await waitFor(() => {
      expect(screen.getByTestId('cloudformation-imports-error')).toBeInTheDocument();
    });
  });
});
