import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { cleanup, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ApiGatewayStagesSection } from './ApiGatewayStagesSection';
import {
  createApiGatewayRestDeployment,
  createApiGatewayRestStage,
  deleteApiGatewayRestStage,
  getApiGatewayRestDeployments,
  getApiGatewayRestStage,
  getApiGatewayRestStages,
  updateApiGatewayRestStage,
} from '../../api/client';
import type {
  ApiGatewayRestDeploymentItem,
  ApiGatewayRestStageItem,
} from '../../api/client';

vi.mock('../../api/client');

const getStagesMock = vi.mocked(getApiGatewayRestStages);
const getStageMock = vi.mocked(getApiGatewayRestStage);
const getDeploymentsMock = vi.mocked(getApiGatewayRestDeployments);
const createDeploymentMock = vi.mocked(createApiGatewayRestDeployment);
const createStageMock = vi.mocked(createApiGatewayRestStage);
const updateStageMock = vi.mocked(updateApiGatewayRestStage);
const deleteStageMock = vi.mocked(deleteApiGatewayRestStage);

const stage: ApiGatewayRestStageItem = {
  stageName: 'dev',
  deploymentId: 'deployment1',
  createdDate: '2024-01-01T00:00:00+00:00',
};

const deployment: ApiGatewayRestDeploymentItem = {
  id: 'deployment1',
  description: 'Initial deployment',
  createdDate: '2024-01-01T00:00:00+00:00',
};

function renderSection() {
  return render(<ApiGatewayStagesSection restApiId="api-1" />);
}

describe('ApiGatewayStagesSection', () => {
  beforeEach(() => {
    getStagesMock.mockResolvedValue({ stages: [stage] });
    getDeploymentsMock.mockResolvedValue({ deployments: [deployment] });
    getStageMock.mockResolvedValue({
      stageName: 'dev',
      deploymentId: 'deployment1',
      description: 'Development stage',
      cacheClusterEnabled: true,
      variables: { logLevel: 'INFO' },
      createdDate: '2024-01-01T00:00:00+00:00',
      lastUpdatedDate: '2024-01-02T00:00:00+00:00',
    });
    createDeploymentMock.mockResolvedValue({ id: 'deployment9' });
    createStageMock.mockResolvedValue({ stageName: 'staging' });
    updateStageMock.mockResolvedValue();
    deleteStageMock.mockResolvedValue();
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows a loading state before data arrives', () => {
    getStagesMock.mockReturnValue(new Promise(() => {}));

    renderSection();

    expect(screen.getByTestId('apigateway-stages-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getStagesMock.mockRejectedValue(new Error('boom'));

    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-stages-error')).toBeInTheDocument(),
    );
  });

  it('shows empty messages when there are no stages or deployments', async () => {
    getStagesMock.mockResolvedValue({ stages: [] });
    getDeploymentsMock.mockResolvedValue({ deployments: [] });

    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-stages-empty')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('apigateway-deployments-empty')).toBeInTheDocument();
  });

  it('lists stages and deployments when the request succeeds', async () => {
    renderSection();

    await waitFor(() => expect(screen.getByTestId('apigateway-stage-dev')).toBeInTheDocument());
    expect(screen.getByTestId('apigateway-stage-name-dev')).toHaveTextContent('dev');
    expect(screen.getByTestId('apigateway-stage-deployment-dev')).toHaveTextContent(
      'deployment1',
    );
    expect(screen.getByTestId('apigateway-stage-url-dev')).toHaveTextContent(
      'https://api-1.execute-api.localhost.localstack.cloud:4566/dev',
    );
    expect(screen.getByTestId('apigateway-deployment-deployment1')).toBeInTheDocument();
    expect(screen.getByTestId('apigateway-deployment-id-deployment1')).toHaveTextContent(
      'deployment1',
    );
    expect(
      screen.getByTestId('apigateway-deployment-description-deployment1'),
    ).toHaveTextContent('Initial deployment');
    expect(screen.getByTestId('apigateway-deployment-created-deployment1')).toHaveTextContent(
      '2024-01-01T00:00:00+00:00',
    );
  });

  it('renders placeholders for deployments with null description and created date', async () => {
    getDeploymentsMock.mockResolvedValue({
      deployments: [{ id: 'deployment1', description: null, createdDate: null }],
    });

    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-deployment-deployment1')).toBeInTheDocument(),
    );
    expect(
      screen.getByTestId('apigateway-deployment-description-deployment1'),
    ).toHaveTextContent('\u2014');
    expect(screen.getByTestId('apigateway-deployment-created-deployment1')).toHaveTextContent(
      '\u2014',
    );
  });

  it('creates a deployment with a description', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-deployment-add')).toBeInTheDocument(),
    );
    await user.type(
      screen.getByTestId('apigateway-deployment-description'),
      'New deployment',
    );
    await user.click(screen.getByTestId('apigateway-deployment-add'));

    await waitFor(() => expect(createDeploymentMock).toHaveBeenCalled());
    expect(createDeploymentMock).toHaveBeenCalledWith('api-1', {
      stageName: null,
      description: 'New deployment',
    });
    expect(getStagesMock).toHaveBeenCalledTimes(2);
  });

  it('creates a deployment with a null description when left blank', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-deployment-add')).toBeInTheDocument(),
    );
    await user.click(screen.getByTestId('apigateway-deployment-add'));

    await waitFor(() => expect(createDeploymentMock).toHaveBeenCalled());
    expect(createDeploymentMock).toHaveBeenCalledWith('api-1', {
      stageName: null,
      description: null,
    });
  });

  it('shows an error when the deployment create request fails', async () => {
    createDeploymentMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-deployment-add')).toBeInTheDocument(),
    );
    await user.click(screen.getByTestId('apigateway-deployment-add'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-deployment-add-error')).toBeInTheDocument(),
    );
  });

  it('disables the add stage button until a name and deployment are chosen', async () => {
    renderSection();

    await waitFor(() => expect(screen.getByTestId('apigateway-stage-add')).toBeInTheDocument());
    expect(screen.getByTestId('apigateway-stage-add')).toBeDisabled();

    const user = userEvent.setup();
    await user.type(screen.getByTestId('apigateway-stage-name'), 'staging');
    expect(screen.getByTestId('apigateway-stage-add')).toBeDisabled();

    await user.selectOptions(screen.getByTestId('apigateway-stage-deployment'), 'deployment1');
    expect(screen.getByTestId('apigateway-stage-add')).toBeEnabled();
  });

  it('creates a stage from the selected deployment', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() => expect(screen.getByTestId('apigateway-stage-add')).toBeInTheDocument());
    await user.type(screen.getByTestId('apigateway-stage-name'), 'staging');
    await user.selectOptions(screen.getByTestId('apigateway-stage-deployment'), 'deployment1');
    await user.type(screen.getByTestId('apigateway-stage-description'), 'Staging stage');
    await user.click(screen.getByTestId('apigateway-stage-add'));

    await waitFor(() => expect(createStageMock).toHaveBeenCalled());
    expect(createStageMock).toHaveBeenCalledWith('api-1', {
      stageName: 'staging',
      deploymentId: 'deployment1',
      description: 'Staging stage',
      variables: null,
    });
    expect(getStagesMock).toHaveBeenCalledTimes(2);
  });

  it('creates a stage with a null description when left blank', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() => expect(screen.getByTestId('apigateway-stage-add')).toBeInTheDocument());
    await user.type(screen.getByTestId('apigateway-stage-name'), 'staging');
    await user.selectOptions(screen.getByTestId('apigateway-stage-deployment'), 'deployment1');
    await user.click(screen.getByTestId('apigateway-stage-add'));

    await waitFor(() => expect(createStageMock).toHaveBeenCalled());
    expect(createStageMock).toHaveBeenCalledWith('api-1', {
      stageName: 'staging',
      deploymentId: 'deployment1',
      description: null,
      variables: null,
    });
  });

  it('shows an error when the stage create request fails', async () => {
    createStageMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderSection();

    await waitFor(() => expect(screen.getByTestId('apigateway-stage-add')).toBeInTheDocument());
    await user.type(screen.getByTestId('apigateway-stage-name'), 'staging');
    await user.selectOptions(screen.getByTestId('apigateway-stage-deployment'), 'deployment1');
    await user.click(screen.getByTestId('apigateway-stage-add'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-stage-add-error')).toBeInTheDocument(),
    );
  });

  it('shows stage detail when View is clicked', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-stage-view-dev')).toBeInTheDocument(),
    );
    await user.click(screen.getByTestId('apigateway-stage-view-dev'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-stage-detail')).toBeInTheDocument(),
    );
    expect(screen.getByTestId('apigateway-stage-detail-deployment')).toHaveTextContent(
      'deployment1',
    );
    expect(screen.getByTestId('apigateway-stage-detail-cache')).toHaveTextContent('Enabled');
    expect(screen.getByTestId('apigateway-stage-detail-created')).toHaveTextContent(
      '2024-01-01T00:00:00+00:00',
    );
    expect(screen.getByTestId('apigateway-stage-detail-updated')).toHaveTextContent(
      '2024-01-02T00:00:00+00:00',
    );
    expect(screen.getByTestId('apigateway-stage-detail-variables')).toHaveTextContent(
      'logLevel=INFO',
    );
    expect(getStageMock).toHaveBeenCalledWith('api-1', 'dev');
  });

  it('renders placeholders when the stage detail has null fields and no variables', async () => {
    getStageMock.mockResolvedValue({
      stageName: 'dev',
      deploymentId: 'deployment1',
      description: null,
      cacheClusterEnabled: false,
      variables: {},
      createdDate: null,
      lastUpdatedDate: null,
    });
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-stage-view-dev')).toBeInTheDocument(),
    );
    await user.click(screen.getByTestId('apigateway-stage-view-dev'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-stage-detail-cache')).toHaveTextContent('Disabled'),
    );
    expect(screen.getByTestId('apigateway-stage-detail-created')).toHaveTextContent('\u2014');
    expect(screen.getByTestId('apigateway-stage-detail-updated')).toHaveTextContent('\u2014');
    expect(
      screen.getByTestId('apigateway-stage-detail-variables-empty'),
    ).toHaveTextContent('\u2014');
  });

  it('keeps the detail hidden when the view request fails', async () => {
    getStageMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-stage-view-dev')).toBeInTheDocument(),
    );
    await user.click(screen.getByTestId('apigateway-stage-view-dev'));

    await waitFor(() => expect(getStageMock).toHaveBeenCalled());
    expect(screen.queryByTestId('apigateway-stage-detail')).not.toBeInTheDocument();
  });

  it('updates a stage from the detail panel', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-stage-view-dev')).toBeInTheDocument(),
    );
    await user.click(screen.getByTestId('apigateway-stage-view-dev'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-stage-edit-save')).toBeInTheDocument(),
    );
    await user.clear(screen.getByTestId('apigateway-stage-edit-description'));
    await user.type(screen.getByTestId('apigateway-stage-edit-description'), 'Updated');
    await user.clear(screen.getByTestId('apigateway-stage-edit-variables'));
    await user.type(screen.getByTestId('apigateway-stage-edit-variables'), 'logLevel=DEBUG');
    await user.click(screen.getByTestId('apigateway-stage-edit-save'));

    await waitFor(() => expect(updateStageMock).toHaveBeenCalled());
    expect(updateStageMock).toHaveBeenCalledWith('api-1', 'dev', {
      description: 'Updated',
      variables: { logLevel: 'DEBUG' },
    });
    expect(screen.getByTestId('apigateway-stage-edit-status')).toBeInTheDocument();
  });

  it('updates a stage with a null description and ignores malformed variable lines', async () => {
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-stage-view-dev')).toBeInTheDocument(),
    );
    await user.click(screen.getByTestId('apigateway-stage-view-dev'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-stage-edit-save')).toBeInTheDocument(),
    );
    await user.clear(screen.getByTestId('apigateway-stage-edit-description'));
    await user.clear(screen.getByTestId('apigateway-stage-edit-variables'));
    await user.type(
      screen.getByTestId('apigateway-stage-edit-variables'),
      'noseparator{enter}=novalue{enter}good=value{enter}   ',
    );
    await user.click(screen.getByTestId('apigateway-stage-edit-save'));

    await waitFor(() => expect(updateStageMock).toHaveBeenCalled());
    expect(updateStageMock).toHaveBeenCalledWith('api-1', 'dev', {
      description: null,
      variables: { good: 'value' },
    });
  });

  it('shows an error when the stage update request fails', async () => {
    updateStageMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-stage-view-dev')).toBeInTheDocument(),
    );
    await user.click(screen.getByTestId('apigateway-stage-view-dev'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-stage-edit-save')).toBeInTheDocument(),
    );
    await user.click(screen.getByTestId('apigateway-stage-edit-save'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-stage-edit-error')).toBeInTheDocument(),
    );
  });

  it('deletes a stage after confirmation', async () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-stage-delete-dev')).toBeInTheDocument(),
    );
    await user.click(screen.getByTestId('apigateway-stage-delete-dev'));

    await waitFor(() => expect(deleteStageMock).toHaveBeenCalledWith('api-1', 'dev'));
    expect(getStagesMock).toHaveBeenCalledTimes(2);
    confirmSpy.mockRestore();
  });

  it('does not delete a stage when confirmation is declined', async () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(false);
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-stage-delete-dev')).toBeInTheDocument(),
    );
    await user.click(screen.getByTestId('apigateway-stage-delete-dev'));

    expect(deleteStageMock).not.toHaveBeenCalled();
    confirmSpy.mockRestore();
  });

  it('shows an error state when the delete request fails', async () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    deleteStageMock.mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderSection();

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-stage-delete-dev')).toBeInTheDocument(),
    );
    await user.click(screen.getByTestId('apigateway-stage-delete-dev'));

    await waitFor(() =>
      expect(screen.getByTestId('apigateway-stages-error')).toBeInTheDocument(),
    );
  });
});
