import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
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
  ApiGatewayRestStageDetailResult,
  ApiGatewayRestStageItem,
} from '../../api/client';

const INVOKE_URL_HOST = 'execute-api.localhost.localstack.cloud:4566';

const sectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const headingStyle: CSSProperties = { fontSize: 15, fontWeight: 600 };
const subHeadingStyle: CSSProperties = { fontSize: 13, fontWeight: 600, opacity: 0.85 };
const messageStyle: CSSProperties = { fontSize: 13 };
const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const valueStyle: CSSProperties = { fontSize: 13, fontFamily: 'monospace' };

const rowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 6,
  padding: 8,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const inlineStyle: CSSProperties = {
  display: 'flex',
  gap: 8,
  alignItems: 'center',
  flexWrap: 'wrap',
};

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
};

const buttonStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
};

type LoadState =
  | { kind: 'loading' }
  | {
      kind: 'ready';
      stages: ApiGatewayRestStageItem[];
      deployments: ApiGatewayRestDeploymentItem[];
    }
  | { kind: 'error' };

function emptyToNull(value: string): string | null {
  const trimmed = value.trim();
  return trimmed === '' ? null : trimmed;
}

function parseVariables(value: string): Record<string, string> {
  const result: Record<string, string> = {};
  for (const line of value.split('\n')) {
    const trimmed = line.trim();
    if (trimmed === '') {
      continue;
    }
    const separator = trimmed.indexOf('=');
    if (separator <= 0) {
      continue;
    }
    const key = trimmed.slice(0, separator).trim();
    const variableValue = trimmed.slice(separator + 1).trim();
    if (key !== '') {
      result[key] = variableValue;
    }
  }
  return result;
}

interface ApiGatewayStagesSectionProps {
  restApiId: string;
}

export function ApiGatewayStagesSection({ restApiId }: ApiGatewayStagesSectionProps) {
  const [state, setState] = useState<LoadState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);

  const [newDeploymentDescription, setNewDeploymentDescription] = useState('');
  const [deploymentError, setDeploymentError] = useState(false);

  const [newStageName, setNewStageName] = useState('');
  const [selectedDeploymentId, setSelectedDeploymentId] = useState('');
  const [newStageDescription, setNewStageDescription] = useState('');
  const [stageError, setStageError] = useState(false);

  const [detail, setDetail] = useState<ApiGatewayRestStageDetailResult | null>(null);

  useEffect(() => {
    const controller = new AbortController();
    setState({ kind: 'loading' });
    Promise.all([
      getApiGatewayRestStages(restApiId, controller.signal),
      getApiGatewayRestDeployments(restApiId, controller.signal),
    ])
      .then(([stagesResult, deploymentsResult]) =>
        setState({
          kind: 'ready',
          stages: stagesResult.stages,
          deployments: deploymentsResult.deployments,
        }),
      )
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [restApiId, reloadToken]);

  const refresh = useCallback(() => {
    setReloadToken((token) => token + 1);
  }, []);

  const handleCreateDeployment = () => {
    setDeploymentError(false);
    createApiGatewayRestDeployment(restApiId, {
      stageName: null,
      description: emptyToNull(newDeploymentDescription),
    })
      .then(() => {
        setNewDeploymentDescription('');
        refresh();
      })
      .catch(() => setDeploymentError(true));
  };

  const handleCreateStage = () => {
    setStageError(false);
    createApiGatewayRestStage(restApiId, {
      stageName: newStageName,
      deploymentId: selectedDeploymentId,
      description: emptyToNull(newStageDescription),
      variables: null,
    })
      .then(() => {
        setNewStageName('');
        setSelectedDeploymentId('');
        setNewStageDescription('');
        refresh();
      })
      .catch(() => setStageError(true));
  };

  const handleDeleteStage = (stageName: string) => {
    if (!window.confirm('Delete this stage?')) {
      return;
    }
    deleteApiGatewayRestStage(restApiId, stageName)
      .then(refresh)
      .catch(() => setState({ kind: 'error' }));
  };

  const handleViewStage = (stageName: string) => {
    setDetail(null);
    getApiGatewayRestStage(restApiId, stageName)
      .then((result) => setDetail(result))
      .catch(() => setDetail(null));
  };

  if (state.kind === 'loading') {
    return (
      <p data-testid="apigateway-stages-loading" style={messageStyle}>
        Loading stages&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="apigateway-stages-error" style={messageStyle}>
        Unable to load stages.
      </p>
    );
  }

  const stages = state.stages;
  const deployments = state.deployments;
  const stageAddDisabled = newStageName.trim() === '' || selectedDeploymentId === '';

  return (
    <div data-testid="apigateway-stages-section" style={sectionStyle}>
      <span style={headingStyle}>Stages &amp; deployments</span>

      <span style={subHeadingStyle}>Deployments</span>

      {deployments.length === 0 ? (
        <p data-testid="apigateway-deployments-empty" style={messageStyle}>
          No deployments found.
        </p>
      ) : null}

      {deployments.map((deployment) => (
        <div
          key={deployment.id}
          data-testid={`apigateway-deployment-${deployment.id}`}
          style={rowStyle}
        >
          <div style={inlineStyle}>
            <span data-testid={`apigateway-deployment-id-${deployment.id}`} style={valueStyle}>
              {deployment.id}
            </span>
            <span
              data-testid={`apigateway-deployment-description-${deployment.id}`}
              style={labelStyle}
            >
              {deployment.description ?? '\u2014'}
            </span>
            <span
              data-testid={`apigateway-deployment-created-${deployment.id}`}
              style={labelStyle}
            >
              {deployment.createdDate ?? '\u2014'}
            </span>
          </div>
        </div>
      ))}

      <div data-testid="apigateway-deployment-add-form" style={inlineStyle}>
        <input
          type="text"
          data-testid="apigateway-deployment-description"
          style={inputStyle}
          placeholder="deployment description (optional)"
          value={newDeploymentDescription}
          onChange={(event) => setNewDeploymentDescription(event.target.value)}
        />
        <button
          type="button"
          data-testid="apigateway-deployment-add"
          style={buttonStyle}
          onClick={handleCreateDeployment}
        >
          Create deployment
        </button>
      </div>

      {deploymentError ? (
        <p data-testid="apigateway-deployment-add-error" style={messageStyle}>
          Unable to create the deployment.
        </p>
      ) : null}

      <span style={subHeadingStyle}>Stages</span>

      {stages.length === 0 ? (
        <p data-testid="apigateway-stages-empty" style={messageStyle}>
          No stages found.
        </p>
      ) : null}

      {stages.map((stage) => (
        <div
          key={stage.stageName}
          data-testid={`apigateway-stage-${stage.stageName}`}
          style={rowStyle}
        >
          <div style={inlineStyle}>
            <span data-testid={`apigateway-stage-name-${stage.stageName}`} style={valueStyle}>
              {stage.stageName}
            </span>
            <span
              data-testid={`apigateway-stage-deployment-${stage.stageName}`}
              style={labelStyle}
            >
              {stage.deploymentId}
            </span>
            <button
              type="button"
              data-testid={`apigateway-stage-view-${stage.stageName}`}
              style={buttonStyle}
              onClick={() => handleViewStage(stage.stageName)}
            >
              View
            </button>
            <button
              type="button"
              data-testid={`apigateway-stage-delete-${stage.stageName}`}
              style={buttonStyle}
              onClick={() => handleDeleteStage(stage.stageName)}
            >
              Delete
            </button>
          </div>
          <div style={inlineStyle}>
            <span style={labelStyle}>Invoke URL:</span>
            <span data-testid={`apigateway-stage-url-${stage.stageName}`} style={valueStyle}>
              {`https://${restApiId}.${INVOKE_URL_HOST}/${stage.stageName}`}
            </span>
          </div>
        </div>
      ))}

      {detail !== null ? (
        <ApiGatewayStageDetailPanel restApiId={restApiId} detail={detail} onSaved={refresh} />
      ) : null}

      <div data-testid="apigateway-stage-add-form" style={inlineStyle}>
        <input
          type="text"
          data-testid="apigateway-stage-name"
          style={inputStyle}
          placeholder="stage name"
          value={newStageName}
          onChange={(event) => setNewStageName(event.target.value)}
        />
        <select
          data-testid="apigateway-stage-deployment"
          style={inputStyle}
          value={selectedDeploymentId}
          onChange={(event) => setSelectedDeploymentId(event.target.value)}
        >
          <option value="">(select deployment)</option>
          {deployments.map((deployment) => (
            <option key={deployment.id} value={deployment.id}>
              {deployment.id}
            </option>
          ))}
        </select>
        <input
          type="text"
          data-testid="apigateway-stage-description"
          style={inputStyle}
          placeholder="description (optional)"
          value={newStageDescription}
          onChange={(event) => setNewStageDescription(event.target.value)}
        />
        <button
          type="button"
          data-testid="apigateway-stage-add"
          style={buttonStyle}
          disabled={stageAddDisabled}
          onClick={handleCreateStage}
        >
          Add stage
        </button>
      </div>

      {stageError ? (
        <p data-testid="apigateway-stage-add-error" style={messageStyle}>
          Unable to add the stage.
        </p>
      ) : null}
    </div>
  );
}

interface ApiGatewayStageDetailPanelProps {
  restApiId: string;
  detail: ApiGatewayRestStageDetailResult;
  onSaved: () => void;
}

function formatVariables(variables: Record<string, string>): string {
  return Object.entries(variables)
    .map(([key, value]) => `${key}=${value}`)
    .join('\n');
}

function ApiGatewayStageDetailPanel({ restApiId, detail, onSaved }: ApiGatewayStageDetailPanelProps) {
  const [description, setDescription] = useState(detail.description ?? '');
  const [variablesText, setVariablesText] = useState(formatVariables(detail.variables));
  const [saveState, setSaveState] = useState<'idle' | 'saving' | 'saved' | 'error'>('idle');

  const variableEntries = Object.entries(detail.variables);

  const handleSave = () => {
    setSaveState('saving');
    updateApiGatewayRestStage(restApiId, detail.stageName, {
      description: emptyToNull(description),
      variables: parseVariables(variablesText),
    })
      .then(() => {
        setSaveState('saved');
        onSaved();
      })
      .catch(() => setSaveState('error'));
  };

  return (
    <div data-testid="apigateway-stage-detail" style={rowStyle}>
      <span style={valueStyle}>{detail.stageName}</span>
      <div style={inlineStyle}>
        <span style={labelStyle}>Deployment:</span>
        <span data-testid="apigateway-stage-detail-deployment" style={valueStyle}>
          {detail.deploymentId}
        </span>
      </div>
      <div style={inlineStyle}>
        <span style={labelStyle}>Cache cluster:</span>
        <span data-testid="apigateway-stage-detail-cache" style={valueStyle}>
          {detail.cacheClusterEnabled ? 'Enabled' : 'Disabled'}
        </span>
      </div>
      <div style={inlineStyle}>
        <span style={labelStyle}>Created:</span>
        <span data-testid="apigateway-stage-detail-created" style={valueStyle}>
          {detail.createdDate ?? '\u2014'}
        </span>
      </div>
      <div style={inlineStyle}>
        <span style={labelStyle}>Last updated:</span>
        <span data-testid="apigateway-stage-detail-updated" style={valueStyle}>
          {detail.lastUpdatedDate ?? '\u2014'}
        </span>
      </div>
      <div style={inlineStyle}>
        <span style={labelStyle}>Variables:</span>
        {variableEntries.length === 0 ? (
          <span data-testid="apigateway-stage-detail-variables-empty" style={valueStyle}>
            {'\u2014'}
          </span>
        ) : (
          <span data-testid="apigateway-stage-detail-variables" style={valueStyle}>
            {variableEntries.map(([key, value]) => `${key}=${value}`).join(', ')}
          </span>
        )}
      </div>
      <input
        type="text"
        data-testid="apigateway-stage-edit-description"
        style={inputStyle}
        placeholder="description (optional)"
        value={description}
        onChange={(event) => setDescription(event.target.value)}
      />
      <textarea
        data-testid="apigateway-stage-edit-variables"
        style={inputStyle}
        placeholder="key=value per line"
        value={variablesText}
        onChange={(event) => setVariablesText(event.target.value)}
      />
      <button
        type="button"
        data-testid="apigateway-stage-edit-save"
        style={buttonStyle}
        disabled={saveState === 'saving'}
        onClick={handleSave}
      >
        {saveState === 'saving' ? 'Saving\u2026' : 'Save stage'}
      </button>
      {saveState === 'saved' ? (
        <p data-testid="apigateway-stage-edit-status" style={messageStyle}>
          Stage updated.
        </p>
      ) : null}
      {saveState === 'error' ? (
        <p data-testid="apigateway-stage-edit-error" style={messageStyle}>
          Unable to update the stage.
        </p>
      ) : null}
    </div>
  );
}

export default ApiGatewayStagesSection;
