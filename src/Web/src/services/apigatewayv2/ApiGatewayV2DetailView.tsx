import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading } from '@primer/react';
import {
  getHttpApi,
  updateHttpApi,
  getHttpRoutes,
  createHttpRoute,
  updateHttpRoute,
  deleteHttpRoute,
  getHttpIntegrations,
  createHttpIntegration,
  getHttpAuthorizers,
  createHttpAuthorizer,
  updateHttpAuthorizer,
  deleteHttpAuthorizer,
  getHttpStages,
  createHttpStage,
  updateHttpStage,
  deleteHttpStage,
} from '../../api/client';
import type {
  HttpApiDetailResult,
  HttpRouteSummaryItem,
  HttpIntegrationSummaryItem,
  HttpAuthorizerSummaryItem,
  HttpStageSummaryItem,
} from '../../api/client';
import type { ServiceDetailViewProps } from '../serviceViewRegistry';
import { RawJsonViewer } from '../../components/RawJsonViewer';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const rowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const valueStyle: CSSProperties = { fontSize: 14, fontFamily: 'monospace' };
const messageStyle: CSSProperties = { fontSize: 14 };
const sectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};

const formStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const fieldRowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
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
  alignSelf: 'flex-start',
};

const protocolTypes = ['HTTP', 'WEBSOCKET'];

type LoadState =
  | { kind: 'loading' }
  | { kind: 'ready'; api: HttpApiDetailResult }
  | { kind: 'error' };

type SaveState = 'idle' | 'saving' | 'saved' | 'error';

type RoutesState =
  | { kind: 'loading' }
  | { kind: 'ready'; routes: HttpRouteSummaryItem[] }
  | { kind: 'error' };

type IntegrationsState =
  | { kind: 'loading' }
  | { kind: 'ready'; integrations: HttpIntegrationSummaryItem[] }
  | { kind: 'error' };

type AuthorizersState =
  | { kind: 'loading' }
  | { kind: 'ready'; authorizers: HttpAuthorizerSummaryItem[] }
  | { kind: 'error' };

type StagesState =
  | { kind: 'loading' }
  | { kind: 'ready'; stages: HttpStageSummaryItem[] }
  | { kind: 'error' };

const integrationTypes = ['AWS', 'AWS_PROXY', 'HTTP', 'HTTP_PROXY', 'MOCK'];
const authorizationTypes = ['NONE', 'AWS_IAM', 'JWT', 'CUSTOM'];

function emptyToNull(value: string): string | null {
  const trimmed = value.trim();
  return trimmed === '' ? null : trimmed;
}

function emptyToNumber(value: string): number | null {
  const trimmed = value.trim();
  return trimmed === '' ? null : Number(trimmed);
}

function scopesToList(value: string): string[] | null {
  const items = value
    .split(',')
    .map((item) => item.trim())
    .filter((item) => item !== '');
  return items.length === 0 ? null : items;
}

function commaToArray(value: string): string[] {
  return value
    .split(',')
    .map((item) => item.trim())
    .filter((item) => item !== '');
}

function formatList(values: string[]): string {
  return values.length === 0 ? '\u2014' : values.join(', ');
}

export function ApiGatewayV2DetailView({ resourceId }: ServiceDetailViewProps) {
  const [state, setState] = useState<LoadState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);

  const [editing, setEditing] = useState(false);
  const [editName, setEditName] = useState('');
  const [editProtocolType, setEditProtocolType] = useState('HTTP');
  const [editDescription, setEditDescription] = useState('');
  const [editVersion, setEditVersion] = useState('');
  const [editRouteSelectionExpression, setEditRouteSelectionExpression] = useState('');
  const [saveState, setSaveState] = useState<SaveState>('idle');

  const [routesState, setRoutesState] = useState<RoutesState>({ kind: 'loading' });
  const [integrationsState, setIntegrationsState] = useState<IntegrationsState>({
    kind: 'loading',
  });
  const [childReloadToken, setChildReloadToken] = useState(0);

  const [newRouteKey, setNewRouteKey] = useState('');
  const [newRouteTarget, setNewRouteTarget] = useState('');
  const [newRouteAuthType, setNewRouteAuthType] = useState('NONE');
  const [newRouteAuthorizerId, setNewRouteAuthorizerId] = useState('');
  const [newRouteScopes, setNewRouteScopes] = useState('');
  const [routeCreateState, setRouteCreateState] = useState<SaveState>('idle');

  const [editingRouteId, setEditingRouteId] = useState<string | null>(null);
  const [editRouteKey, setEditRouteKey] = useState('');
  const [editRouteTarget, setEditRouteTarget] = useState('');
  const [editRouteAuthType, setEditRouteAuthType] = useState('NONE');
  const [editRouteAuthorizerId, setEditRouteAuthorizerId] = useState('');
  const [editRouteScopes, setEditRouteScopes] = useState('');
  const [routeEditState, setRouteEditState] = useState<SaveState>('idle');

  const [pendingDeleteRouteId, setPendingDeleteRouteId] = useState<string | null>(null);
  const [routeDeleteState, setRouteDeleteState] = useState<SaveState>('idle');

  const [newIntegrationType, setNewIntegrationType] = useState('HTTP_PROXY');
  const [newIntegrationMethod, setNewIntegrationMethod] = useState('');
  const [newIntegrationUri, setNewIntegrationUri] = useState('');
  const [newIntegrationPayloadVersion, setNewIntegrationPayloadVersion] = useState('');
  const [newIntegrationDescription, setNewIntegrationDescription] = useState('');
  const [integrationCreateState, setIntegrationCreateState] = useState<SaveState>('idle');

  const [authorizersState, setAuthorizersState] = useState<AuthorizersState>({
    kind: 'loading',
  });
  const [newAuthorizerName, setNewAuthorizerName] = useState('');
  const [newAuthorizerIdentitySource, setNewAuthorizerIdentitySource] = useState(
    '$request.header.Authorization',
  );
  const [newAuthorizerJwtIssuer, setNewAuthorizerJwtIssuer] = useState('');
  const [newAuthorizerJwtAudience, setNewAuthorizerJwtAudience] = useState('');
  const [authorizerCreateState, setAuthorizerCreateState] = useState<SaveState>('idle');

  const [editingAuthorizerId, setEditingAuthorizerId] = useState<string | null>(null);
  const [editAuthorizerName, setEditAuthorizerName] = useState('');
  const [editAuthorizerIdentitySource, setEditAuthorizerIdentitySource] = useState('');
  const [editAuthorizerJwtIssuer, setEditAuthorizerJwtIssuer] = useState('');
  const [editAuthorizerJwtAudience, setEditAuthorizerJwtAudience] = useState('');
  const [authorizerEditState, setAuthorizerEditState] = useState<SaveState>('idle');

  const [pendingDeleteAuthorizerId, setPendingDeleteAuthorizerId] = useState<string | null>(
    null,
  );
  const [authorizerDeleteState, setAuthorizerDeleteState] = useState<SaveState>('idle');

  const [stagesState, setStagesState] = useState<StagesState>({ kind: 'loading' });
  const [newStageName, setNewStageName] = useState('');
  const [newStageAutoDeploy, setNewStageAutoDeploy] = useState(true);
  const [newStageDescription, setNewStageDescription] = useState('');
  const [newStageBurstLimit, setNewStageBurstLimit] = useState('');
  const [newStageRateLimit, setNewStageRateLimit] = useState('');
  const [stageCreateState, setStageCreateState] = useState<SaveState>('idle');

  const [editingStageName, setEditingStageName] = useState<string | null>(null);
  const [editStageAutoDeploy, setEditStageAutoDeploy] = useState(true);
  const [editStageDescription, setEditStageDescription] = useState('');
  const [editStageBurstLimit, setEditStageBurstLimit] = useState('');
  const [editStageRateLimit, setEditStageRateLimit] = useState('');
  const [stageEditState, setStageEditState] = useState<SaveState>('idle');

  const [pendingDeleteStageName, setPendingDeleteStageName] = useState<string | null>(null);
  const [stageDeleteState, setStageDeleteState] = useState<SaveState>('idle');

  useEffect(() => {
    const controller = new AbortController();
    setState({ kind: 'loading' });
    getHttpApi(resourceId, controller.signal)
      .then((api) => setState({ kind: 'ready', api }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [resourceId, reloadToken]);

  useEffect(() => {
    const controller = new AbortController();
    setRoutesState({ kind: 'loading' });
    getHttpRoutes(resourceId, controller.signal)
      .then((result) => setRoutesState({ kind: 'ready', routes: result.routes }))
      .catch(() => setRoutesState({ kind: 'error' }));
    return () => controller.abort();
  }, [resourceId, childReloadToken]);

  useEffect(() => {
    const controller = new AbortController();
    setIntegrationsState({ kind: 'loading' });
    getHttpIntegrations(resourceId, controller.signal)
      .then((result) =>
        setIntegrationsState({ kind: 'ready', integrations: result.integrations }),
      )
      .catch(() => setIntegrationsState({ kind: 'error' }));
    return () => controller.abort();
  }, [resourceId, childReloadToken]);

  useEffect(() => {
    const controller = new AbortController();
    setAuthorizersState({ kind: 'loading' });
    getHttpAuthorizers(resourceId, controller.signal)
      .then((result) =>
        setAuthorizersState({ kind: 'ready', authorizers: result.authorizers }),
      )
      .catch(() => setAuthorizersState({ kind: 'error' }));
    return () => controller.abort();
  }, [resourceId, childReloadToken]);

  useEffect(() => {
    const controller = new AbortController();
    setStagesState({ kind: 'loading' });
    getHttpStages(resourceId, controller.signal)
      .then((result) => setStagesState({ kind: 'ready', stages: result.stages }))
      .catch(() => setStagesState({ kind: 'error' }));
    return () => controller.abort();
  }, [resourceId, childReloadToken]);

  const refresh = useCallback(() => {
    setReloadToken((token) => token + 1);
  }, []);

  const refreshChildren = useCallback(() => {
    setChildReloadToken((token) => token + 1);
  }, []);

  const handleStartEdit = (api: HttpApiDetailResult) => {
    setEditName(api.name);
    setEditProtocolType(api.protocolType);
    setEditDescription(api.description ?? '');
    setEditVersion(api.version ?? '');
    setEditRouteSelectionExpression(api.routeSelectionExpression ?? '');
    setSaveState('idle');
    setEditing(true);
  };

  const handleUpdate = () => {
    setSaveState('saving');
    updateHttpApi(resourceId, {
      name: editName,
      protocolType: editProtocolType,
      description: emptyToNull(editDescription),
      version: emptyToNull(editVersion),
      routeSelectionExpression: emptyToNull(editRouteSelectionExpression),
    })
      .then(() => {
        setSaveState('saved');
        setEditing(false);
        refresh();
      })
      .catch(() => setSaveState('error'));
  };

  const handleCreateRoute = () => {
    setRouteCreateState('saving');
    createHttpRoute(resourceId, {
      routeKey: newRouteKey,
      target: emptyToNull(newRouteTarget),
      authorizationType: emptyToNull(newRouteAuthType),
      authorizerId: emptyToNull(newRouteAuthorizerId),
      authorizationScopes: scopesToList(newRouteScopes),
    })
      .then(() => {
        setRouteCreateState('saved');
        setNewRouteKey('');
        setNewRouteTarget('');
        setNewRouteAuthType('NONE');
        setNewRouteAuthorizerId('');
        setNewRouteScopes('');
        refreshChildren();
      })
      .catch(() => setRouteCreateState('error'));
  };

  const handleStartEditRoute = (route: HttpRouteSummaryItem) => {
    setEditingRouteId(route.routeId);
    setEditRouteKey(route.routeKey);
    setEditRouteTarget(route.target ?? '');
    setEditRouteAuthType(route.authorizationType ?? 'NONE');
    setEditRouteAuthorizerId('');
    setEditRouteScopes('');
    setRouteEditState('idle');
  };

  const handleUpdateRoute = (routeId: string) => {
    setRouteEditState('saving');
    updateHttpRoute(resourceId, routeId, {
      routeKey: editRouteKey,
      target: emptyToNull(editRouteTarget),
      authorizationType: emptyToNull(editRouteAuthType),
      authorizerId: emptyToNull(editRouteAuthorizerId),
      authorizationScopes: scopesToList(editRouteScopes),
    })
      .then(() => {
        setRouteEditState('saved');
        setEditingRouteId(null);
        refreshChildren();
      })
      .catch(() => setRouteEditState('error'));
  };

  const handleDeleteRoute = (routeId: string) => {
    setRouteDeleteState('saving');
    deleteHttpRoute(resourceId, routeId)
      .then(() => {
        setRouteDeleteState('saved');
        setPendingDeleteRouteId(null);
        refreshChildren();
      })
      .catch(() => setRouteDeleteState('error'));
  };

  const handleCreateIntegration = () => {
    setIntegrationCreateState('saving');
    createHttpIntegration(resourceId, {
      integrationType: newIntegrationType,
      integrationMethod: emptyToNull(newIntegrationMethod),
      integrationUri: emptyToNull(newIntegrationUri),
      payloadFormatVersion: emptyToNull(newIntegrationPayloadVersion),
      description: emptyToNull(newIntegrationDescription),
    })
      .then(() => {
        setIntegrationCreateState('saved');
        setNewIntegrationMethod('');
        setNewIntegrationUri('');
        setNewIntegrationPayloadVersion('');
        setNewIntegrationDescription('');
        refreshChildren();
      })
      .catch(() => setIntegrationCreateState('error'));
  };

  const handleCreateAuthorizer = () => {
    setAuthorizerCreateState('saving');
    createHttpAuthorizer(resourceId, {
      name: newAuthorizerName,
      authorizerType: 'JWT',
      identitySource: commaToArray(newAuthorizerIdentitySource),
      jwtIssuer: emptyToNull(newAuthorizerJwtIssuer),
      jwtAudience: commaToArray(newAuthorizerJwtAudience),
    })
      .then(() => {
        setAuthorizerCreateState('saved');
        setNewAuthorizerName('');
        setNewAuthorizerJwtIssuer('');
        setNewAuthorizerJwtAudience('');
        refreshChildren();
      })
      .catch(() => setAuthorizerCreateState('error'));
  };

  const handleStartEditAuthorizer = (authorizer: HttpAuthorizerSummaryItem) => {
    setEditingAuthorizerId(authorizer.authorizerId);
    setEditAuthorizerName(authorizer.name);
    setEditAuthorizerIdentitySource('$request.header.Authorization');
    setEditAuthorizerJwtIssuer('');
    setEditAuthorizerJwtAudience('');
    setAuthorizerEditState('idle');
  };

  const handleUpdateAuthorizer = (authorizerId: string) => {
    setAuthorizerEditState('saving');
    updateHttpAuthorizer(resourceId, authorizerId, {
      name: editAuthorizerName,
      authorizerType: 'JWT',
      identitySource: commaToArray(editAuthorizerIdentitySource),
      jwtIssuer: emptyToNull(editAuthorizerJwtIssuer),
      jwtAudience: commaToArray(editAuthorizerJwtAudience),
    })
      .then(() => {
        setAuthorizerEditState('saved');
        setEditingAuthorizerId(null);
        refreshChildren();
      })
      .catch(() => setAuthorizerEditState('error'));
  };

  const handleDeleteAuthorizer = (authorizerId: string) => {
    setAuthorizerDeleteState('saving');
    deleteHttpAuthorizer(resourceId, authorizerId)
      .then(() => {
        setAuthorizerDeleteState('saved');
        setPendingDeleteAuthorizerId(null);
        refreshChildren();
      })
      .catch(() => setAuthorizerDeleteState('error'));
  };

  const handleCreateStage = () => {
    setStageCreateState('saving');
    createHttpStage(resourceId, {
      stageName: newStageName,
      autoDeploy: newStageAutoDeploy,
      description: emptyToNull(newStageDescription),
      defaultRouteThrottlingBurstLimit: emptyToNumber(newStageBurstLimit),
      defaultRouteThrottlingRateLimit: emptyToNumber(newStageRateLimit),
      stageVariables: {},
    })
      .then(() => {
        setStageCreateState('saved');
        setNewStageName('');
        setNewStageAutoDeploy(true);
        setNewStageDescription('');
        setNewStageBurstLimit('');
        setNewStageRateLimit('');
        refreshChildren();
      })
      .catch(() => setStageCreateState('error'));
  };

  const handleStartEditStage = (stage: HttpStageSummaryItem) => {
    setEditingStageName(stage.stageName);
    setEditStageAutoDeploy(stage.autoDeploy);
    setEditStageDescription('');
    setEditStageBurstLimit('');
    setEditStageRateLimit('');
    setStageEditState('idle');
  };

  const handleUpdateStage = (stageName: string) => {
    setStageEditState('saving');
    updateHttpStage(resourceId, stageName, {
      autoDeploy: editStageAutoDeploy,
      description: emptyToNull(editStageDescription),
      defaultRouteThrottlingBurstLimit: emptyToNumber(editStageBurstLimit),
      defaultRouteThrottlingRateLimit: emptyToNumber(editStageRateLimit),
      stageVariables: {},
    })
      .then(() => {
        setStageEditState('saved');
        setEditingStageName(null);
        refreshChildren();
      })
      .catch(() => setStageEditState('error'));
  };

  const handleDeleteStage = (stageName: string) => {
    setStageDeleteState('saving');
    deleteHttpStage(resourceId, stageName)
      .then(() => {
        setStageDeleteState('saved');
        setPendingDeleteStageName(null);
        refreshChildren();
      })
      .catch(() => setStageDeleteState('error'));
  };

  if (state.kind === 'loading') {
    return (
      <p data-testid="apigatewayv2-detail-loading" style={messageStyle}>
        Loading API&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="apigatewayv2-detail-error" style={messageStyle}>
        Unable to load the API.
      </p>
    );
  }

  const api = state.api;

  return (
    <div data-testid="apigatewayv2-detail-view" style={containerStyle}>
      <Heading as="h2" data-testid="apigatewayv2-detail-name" style={{ fontSize: 18 }}>
        {api.name}
      </Heading>
      <div style={rowStyle}>
        <span style={labelStyle}>API ID</span>
        <span data-testid="apigatewayv2-detail-id" style={valueStyle}>
          {api.apiId}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Protocol type</span>
        <span data-testid="apigatewayv2-detail-protocol" style={valueStyle}>
          {api.protocolType}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Endpoint</span>
        <span data-testid="apigatewayv2-detail-endpoint" style={valueStyle}>
          {api.apiEndpoint ?? '\u2014'}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Description</span>
        <span data-testid="apigatewayv2-detail-description" style={valueStyle}>
          {api.description ?? '\u2014'}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Version</span>
        <span data-testid="apigatewayv2-detail-version" style={valueStyle}>
          {api.version ?? '\u2014'}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Route selection expression</span>
        <span data-testid="apigatewayv2-detail-route-selection" style={valueStyle}>
          {api.routeSelectionExpression ?? '\u2014'}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Created</span>
        <span data-testid="apigatewayv2-detail-created" style={valueStyle}>
          {api.createdDate ?? '\u2014'}
        </span>
      </div>
      {api.corsConfiguration ? (
        <div data-testid="apigatewayv2-detail-cors" style={rowStyle}>
          <span style={labelStyle}>CORS configuration</span>
          <span data-testid="apigatewayv2-detail-cors-origins" style={valueStyle}>
            Allow origins: {formatList(api.corsConfiguration.allowOrigins)}
          </span>
          <span data-testid="apigatewayv2-detail-cors-methods" style={valueStyle}>
            Allow methods: {formatList(api.corsConfiguration.allowMethods)}
          </span>
          <span data-testid="apigatewayv2-detail-cors-headers" style={valueStyle}>
            Allow headers: {formatList(api.corsConfiguration.allowHeaders)}
          </span>
          <span data-testid="apigatewayv2-detail-cors-expose" style={valueStyle}>
            Expose headers: {formatList(api.corsConfiguration.exposeHeaders)}
          </span>
          <span data-testid="apigatewayv2-detail-cors-credentials" style={valueStyle}>
            Allow credentials: {api.corsConfiguration.allowCredentials === null
              ? '\u2014'
              : String(api.corsConfiguration.allowCredentials)}
          </span>
          <span data-testid="apigatewayv2-detail-cors-max-age" style={valueStyle}>
            Max age: {api.corsConfiguration.maxAge ?? '\u2014'}
          </span>
        </div>
      ) : null}

      <button
        type="button"
        data-testid="apigatewayv2-edit-toggle"
        style={buttonStyle}
        onClick={() => (editing ? setEditing(false) : handleStartEdit(api))}
      >
        {editing ? 'Cancel' : 'Edit API'}
      </button>

      {editing ? (
        <div data-testid="apigatewayv2-edit-form" style={formStyle}>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-edit-name">
              Name
            </label>
            <input
              id="apigatewayv2-edit-name"
              type="text"
              data-testid="apigatewayv2-edit-name"
              style={inputStyle}
              value={editName}
              onChange={(event) => setEditName(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-edit-protocol">
              Protocol type
            </label>
            <select
              id="apigatewayv2-edit-protocol"
              data-testid="apigatewayv2-edit-protocol"
              style={inputStyle}
              value={editProtocolType}
              onChange={(event) => setEditProtocolType(event.target.value)}
            >
              {protocolTypes.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </select>
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-edit-description">
              Description
            </label>
            <input
              id="apigatewayv2-edit-description"
              type="text"
              data-testid="apigatewayv2-edit-description"
              style={inputStyle}
              value={editDescription}
              onChange={(event) => setEditDescription(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-edit-version">
              Version
            </label>
            <input
              id="apigatewayv2-edit-version"
              type="text"
              data-testid="apigatewayv2-edit-version"
              style={inputStyle}
              value={editVersion}
              onChange={(event) => setEditVersion(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-edit-route-selection">
              Route selection expression
            </label>
            <input
              id="apigatewayv2-edit-route-selection"
              type="text"
              data-testid="apigatewayv2-edit-route-selection"
              style={inputStyle}
              value={editRouteSelectionExpression}
              onChange={(event) => setEditRouteSelectionExpression(event.target.value)}
            />
          </div>
          <button
            type="button"
            data-testid="apigatewayv2-edit-submit"
            style={buttonStyle}
            disabled={saveState === 'saving'}
            onClick={handleUpdate}
          >
            {saveState === 'saving' ? 'Saving\u2026' : 'Save'}
          </button>
        </div>
      ) : null}

      {saveState === 'saved' ? (
        <p data-testid="apigatewayv2-edit-status" style={messageStyle}>
          API updated.
        </p>
      ) : null}
      {saveState === 'error' ? (
        <p data-testid="apigatewayv2-edit-error" style={messageStyle}>
          Unable to update the API.
        </p>
      ) : null}

      <div data-testid="apigatewayv2-routes-section" style={sectionStyle}>
        <Heading as="h3" style={{ fontSize: 15 }}>
          Routes
        </Heading>
        {routesState.kind === 'loading' ? (
          <p data-testid="apigatewayv2-routes-loading" style={messageStyle}>
            Loading routes&hellip;
          </p>
        ) : null}
        {routesState.kind === 'error' ? (
          <p data-testid="apigatewayv2-routes-error" style={messageStyle}>
            Unable to load routes.
          </p>
        ) : null}
        {routesState.kind === 'ready' && routesState.routes.length === 0 ? (
          <p data-testid="apigatewayv2-routes-empty" style={messageStyle}>
            No routes defined.
          </p>
        ) : null}
        {routesState.kind === 'ready'
          ? routesState.routes.map((route) => (
              <div
                key={route.routeId}
                data-testid={`apigatewayv2-route-${route.routeId}`}
                style={formStyle}
              >
                <span data-testid="apigatewayv2-route-key" style={valueStyle}>
                  {route.routeKey}
                </span>
                <span style={labelStyle}>Route ID: {route.routeId}</span>
                <span style={labelStyle}>Target: {route.target ?? '\u2014'}</span>
                <span style={labelStyle}>
                  Authorization: {route.authorizationType ?? '\u2014'}
                </span>
                {editingRouteId === route.routeId ? (
                  <div data-testid="apigatewayv2-route-edit-form" style={fieldRowStyle}>
                    <label style={labelStyle} htmlFor="apigatewayv2-route-edit-key">
                      Route key
                    </label>
                    <input
                      id="apigatewayv2-route-edit-key"
                      type="text"
                      data-testid="apigatewayv2-route-edit-key"
                      style={inputStyle}
                      value={editRouteKey}
                      onChange={(event) => setEditRouteKey(event.target.value)}
                    />
                    <label style={labelStyle} htmlFor="apigatewayv2-route-edit-target">
                      Target
                    </label>
                    <input
                      id="apigatewayv2-route-edit-target"
                      type="text"
                      data-testid="apigatewayv2-route-edit-target"
                      style={inputStyle}
                      value={editRouteTarget}
                      onChange={(event) => setEditRouteTarget(event.target.value)}
                    />
                    <label style={labelStyle} htmlFor="apigatewayv2-route-edit-auth">
                      Authorization type
                    </label>
                    <select
                      id="apigatewayv2-route-edit-auth"
                      data-testid="apigatewayv2-route-edit-auth"
                      style={inputStyle}
                      value={editRouteAuthType}
                      onChange={(event) => setEditRouteAuthType(event.target.value)}
                    >
                      {authorizationTypes.map((type) => (
                        <option key={type} value={type}>
                          {type}
                        </option>
                      ))}
                    </select>
                    <label style={labelStyle} htmlFor="apigatewayv2-route-edit-authorizer">
                      Authorizer ID
                    </label>
                    <input
                      id="apigatewayv2-route-edit-authorizer"
                      type="text"
                      data-testid="apigatewayv2-route-edit-authorizer"
                      style={inputStyle}
                      value={editRouteAuthorizerId}
                      onChange={(event) => setEditRouteAuthorizerId(event.target.value)}
                    />
                    <label style={labelStyle} htmlFor="apigatewayv2-route-edit-scopes">
                      Authorization scopes (comma separated)
                    </label>
                    <input
                      id="apigatewayv2-route-edit-scopes"
                      type="text"
                      data-testid="apigatewayv2-route-edit-scopes"
                      style={inputStyle}
                      value={editRouteScopes}
                      onChange={(event) => setEditRouteScopes(event.target.value)}
                    />
                    <button
                      type="button"
                      data-testid="apigatewayv2-route-edit-submit"
                      style={buttonStyle}
                      disabled={routeEditState === 'saving'}
                      onClick={() => handleUpdateRoute(route.routeId)}
                    >
                      {routeEditState === 'saving' ? 'Saving\u2026' : 'Save route'}
                    </button>
                    <button
                      type="button"
                      data-testid="apigatewayv2-route-edit-cancel"
                      style={buttonStyle}
                      onClick={() => setEditingRouteId(null)}
                    >
                      Cancel
                    </button>
                  </div>
                ) : (
                  <div style={fieldRowStyle}>
                    <button
                      type="button"
                      data-testid="apigatewayv2-route-edit-toggle"
                      style={buttonStyle}
                      onClick={() => handleStartEditRoute(route)}
                    >
                      Edit route
                    </button>
                    {pendingDeleteRouteId === route.routeId ? (
                      <div data-testid="apigatewayv2-route-delete-confirm" style={fieldRowStyle}>
                        <span style={messageStyle}>Delete this route?</span>
                        <button
                          type="button"
                          data-testid="apigatewayv2-route-delete-confirm-yes"
                          style={buttonStyle}
                          disabled={routeDeleteState === 'saving'}
                          onClick={() => handleDeleteRoute(route.routeId)}
                        >
                          Confirm delete
                        </button>
                        <button
                          type="button"
                          data-testid="apigatewayv2-route-delete-confirm-no"
                          style={buttonStyle}
                          onClick={() => setPendingDeleteRouteId(null)}
                        >
                          Cancel
                        </button>
                      </div>
                    ) : (
                      <button
                        type="button"
                        data-testid="apigatewayv2-route-delete-toggle"
                        style={buttonStyle}
                        onClick={() => setPendingDeleteRouteId(route.routeId)}
                      >
                        Delete route
                      </button>
                    )}
                  </div>
                )}
              </div>
            ))
          : null}
        {routeDeleteState === 'error' ? (
          <p data-testid="apigatewayv2-route-delete-error" style={messageStyle}>
            Unable to delete the route.
          </p>
        ) : null}
        {routeEditState === 'error' ? (
          <p data-testid="apigatewayv2-route-edit-error" style={messageStyle}>
            Unable to update the route.
          </p>
        ) : null}

        <div data-testid="apigatewayv2-route-create-form" style={formStyle}>
          <Heading as="h4" style={{ fontSize: 13 }}>
            Add route
          </Heading>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-route-new-key">
              Route key
            </label>
            <input
              id="apigatewayv2-route-new-key"
              type="text"
              data-testid="apigatewayv2-route-new-key"
              style={inputStyle}
              value={newRouteKey}
              onChange={(event) => setNewRouteKey(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-route-new-target">
              Target
            </label>
            <select
              id="apigatewayv2-route-new-target"
              data-testid="apigatewayv2-route-new-target"
              style={inputStyle}
              value={newRouteTarget}
              onChange={(event) => setNewRouteTarget(event.target.value)}
            >
              <option value="">{'\u2014'}</option>
              {integrationsState.kind === 'ready'
                ? integrationsState.integrations.map((integration) => (
                    <option
                      key={integration.integrationId}
                      value={`integrations/${integration.integrationId}`}
                    >
                      integrations/{integration.integrationId}
                    </option>
                  ))
                : null}
            </select>
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-route-new-auth">
              Authorization type
            </label>
            <select
              id="apigatewayv2-route-new-auth"
              data-testid="apigatewayv2-route-new-auth"
              style={inputStyle}
              value={newRouteAuthType}
              onChange={(event) => setNewRouteAuthType(event.target.value)}
            >
              {authorizationTypes.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </select>
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-route-new-authorizer">
              Authorizer ID
            </label>
            <input
              id="apigatewayv2-route-new-authorizer"
              type="text"
              data-testid="apigatewayv2-route-new-authorizer"
              style={inputStyle}
              value={newRouteAuthorizerId}
              onChange={(event) => setNewRouteAuthorizerId(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-route-new-scopes">
              Authorization scopes (comma separated)
            </label>
            <input
              id="apigatewayv2-route-new-scopes"
              type="text"
              data-testid="apigatewayv2-route-new-scopes"
              style={inputStyle}
              value={newRouteScopes}
              onChange={(event) => setNewRouteScopes(event.target.value)}
            />
          </div>
          <button
            type="button"
            data-testid="apigatewayv2-route-new-submit"
            style={buttonStyle}
            disabled={routeCreateState === 'saving' || newRouteKey.trim() === ''}
            onClick={handleCreateRoute}
          >
            {routeCreateState === 'saving' ? 'Adding\u2026' : 'Add route'}
          </button>
          {routeCreateState === 'error' ? (
            <p data-testid="apigatewayv2-route-new-error" style={messageStyle}>
              Unable to add the route.
            </p>
          ) : null}
        </div>
      </div>

      <div data-testid="apigatewayv2-integrations-section" style={sectionStyle}>
        <Heading as="h3" style={{ fontSize: 15 }}>
          Integrations
        </Heading>
        {integrationsState.kind === 'loading' ? (
          <p data-testid="apigatewayv2-integrations-loading" style={messageStyle}>
            Loading integrations&hellip;
          </p>
        ) : null}
        {integrationsState.kind === 'error' ? (
          <p data-testid="apigatewayv2-integrations-error" style={messageStyle}>
            Unable to load integrations.
          </p>
        ) : null}
        {integrationsState.kind === 'ready' && integrationsState.integrations.length === 0 ? (
          <p data-testid="apigatewayv2-integrations-empty" style={messageStyle}>
            No integrations defined.
          </p>
        ) : null}
        {integrationsState.kind === 'ready'
          ? integrationsState.integrations.map((integration) => (
              <div
                key={integration.integrationId}
                data-testid={`apigatewayv2-integration-${integration.integrationId}`}
                style={formStyle}
              >
                <span data-testid="apigatewayv2-integration-id" style={valueStyle}>
                  {integration.integrationId}
                </span>
                <span style={labelStyle}>Type: {integration.integrationType}</span>
                <span style={labelStyle}>
                  Method: {integration.integrationMethod ?? '\u2014'}
                </span>
                <span style={labelStyle}>URI: {integration.integrationUri ?? '\u2014'}</span>
                <span style={labelStyle}>
                  Payload version: {integration.payloadFormatVersion ?? '\u2014'}
                </span>
                <span style={labelStyle}>
                  Description: {integration.description ?? '\u2014'}
                </span>
              </div>
            ))
          : null}

        <div data-testid="apigatewayv2-integration-create-form" style={formStyle}>
          <Heading as="h4" style={{ fontSize: 13 }}>
            Add integration
          </Heading>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-integration-new-type">
              Integration type
            </label>
            <select
              id="apigatewayv2-integration-new-type"
              data-testid="apigatewayv2-integration-new-type"
              style={inputStyle}
              value={newIntegrationType}
              onChange={(event) => setNewIntegrationType(event.target.value)}
            >
              {integrationTypes.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </select>
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-integration-new-method">
              Integration method
            </label>
            <input
              id="apigatewayv2-integration-new-method"
              type="text"
              data-testid="apigatewayv2-integration-new-method"
              style={inputStyle}
              value={newIntegrationMethod}
              onChange={(event) => setNewIntegrationMethod(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-integration-new-uri">
              Integration URI
            </label>
            <input
              id="apigatewayv2-integration-new-uri"
              type="text"
              data-testid="apigatewayv2-integration-new-uri"
              style={inputStyle}
              value={newIntegrationUri}
              onChange={(event) => setNewIntegrationUri(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-integration-new-payload">
              Payload format version
            </label>
            <input
              id="apigatewayv2-integration-new-payload"
              type="text"
              data-testid="apigatewayv2-integration-new-payload"
              style={inputStyle}
              value={newIntegrationPayloadVersion}
              onChange={(event) => setNewIntegrationPayloadVersion(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-integration-new-description">
              Description
            </label>
            <input
              id="apigatewayv2-integration-new-description"
              type="text"
              data-testid="apigatewayv2-integration-new-description"
              style={inputStyle}
              value={newIntegrationDescription}
              onChange={(event) => setNewIntegrationDescription(event.target.value)}
            />
          </div>
          <button
            type="button"
            data-testid="apigatewayv2-integration-new-submit"
            style={buttonStyle}
            disabled={integrationCreateState === 'saving'}
            onClick={handleCreateIntegration}
          >
            {integrationCreateState === 'saving' ? 'Adding\u2026' : 'Add integration'}
          </button>
          {integrationCreateState === 'error' ? (
            <p data-testid="apigatewayv2-integration-new-error" style={messageStyle}>
              Unable to add the integration.
            </p>
          ) : null}
        </div>
      </div>

      <div data-testid="apigatewayv2-authorizers-section" style={sectionStyle}>
        <Heading as="h3" style={{ fontSize: 15 }}>
          Authorizers
        </Heading>
        {authorizersState.kind === 'loading' ? (
          <p data-testid="apigatewayv2-authorizers-loading" style={messageStyle}>
            Loading authorizers&hellip;
          </p>
        ) : null}
        {authorizersState.kind === 'error' ? (
          <p data-testid="apigatewayv2-authorizers-error" style={messageStyle}>
            Unable to load authorizers.
          </p>
        ) : null}
        {authorizersState.kind === 'ready' && authorizersState.authorizers.length === 0 ? (
          <p data-testid="apigatewayv2-authorizers-empty" style={messageStyle}>
            No authorizers defined.
          </p>
        ) : null}
        {authorizersState.kind === 'ready'
          ? authorizersState.authorizers.map((authorizer) => (
              <div
                key={authorizer.authorizerId}
                data-testid={`apigatewayv2-authorizer-${authorizer.authorizerId}`}
                style={formStyle}
              >
                <span data-testid="apigatewayv2-authorizer-name" style={valueStyle}>
                  {authorizer.name}
                </span>
                <span style={labelStyle}>Authorizer ID: {authorizer.authorizerId}</span>
                <span style={labelStyle}>Type: {authorizer.authorizerType}</span>
                {editingAuthorizerId === authorizer.authorizerId ? (
                  <div data-testid="apigatewayv2-authorizer-edit-form" style={fieldRowStyle}>
                    <label style={labelStyle} htmlFor="apigatewayv2-authorizer-edit-name">
                      Name
                    </label>
                    <input
                      id="apigatewayv2-authorizer-edit-name"
                      type="text"
                      data-testid="apigatewayv2-authorizer-edit-name"
                      style={inputStyle}
                      value={editAuthorizerName}
                      onChange={(event) => setEditAuthorizerName(event.target.value)}
                    />
                    <label style={labelStyle} htmlFor="apigatewayv2-authorizer-edit-identity">
                      Identity source (comma separated)
                    </label>
                    <input
                      id="apigatewayv2-authorizer-edit-identity"
                      type="text"
                      data-testid="apigatewayv2-authorizer-edit-identity"
                      style={inputStyle}
                      value={editAuthorizerIdentitySource}
                      onChange={(event) => setEditAuthorizerIdentitySource(event.target.value)}
                    />
                    <label style={labelStyle} htmlFor="apigatewayv2-authorizer-edit-issuer">
                      JWT issuer
                    </label>
                    <input
                      id="apigatewayv2-authorizer-edit-issuer"
                      type="text"
                      data-testid="apigatewayv2-authorizer-edit-issuer"
                      style={inputStyle}
                      value={editAuthorizerJwtIssuer}
                      onChange={(event) => setEditAuthorizerJwtIssuer(event.target.value)}
                    />
                    <label style={labelStyle} htmlFor="apigatewayv2-authorizer-edit-audience">
                      JWT audience (comma separated)
                    </label>
                    <input
                      id="apigatewayv2-authorizer-edit-audience"
                      type="text"
                      data-testid="apigatewayv2-authorizer-edit-audience"
                      style={inputStyle}
                      value={editAuthorizerJwtAudience}
                      onChange={(event) => setEditAuthorizerJwtAudience(event.target.value)}
                    />
                    <button
                      type="button"
                      data-testid="apigatewayv2-authorizer-edit-submit"
                      style={buttonStyle}
                      disabled={authorizerEditState === 'saving'}
                      onClick={() => handleUpdateAuthorizer(authorizer.authorizerId)}
                    >
                      {authorizerEditState === 'saving' ? 'Saving\u2026' : 'Save authorizer'}
                    </button>
                    <button
                      type="button"
                      data-testid="apigatewayv2-authorizer-edit-cancel"
                      style={buttonStyle}
                      onClick={() => setEditingAuthorizerId(null)}
                    >
                      Cancel
                    </button>
                  </div>
                ) : (
                  <div style={fieldRowStyle}>
                    <button
                      type="button"
                      data-testid="apigatewayv2-authorizer-edit-toggle"
                      style={buttonStyle}
                      onClick={() => handleStartEditAuthorizer(authorizer)}
                    >
                      Edit authorizer
                    </button>
                    {pendingDeleteAuthorizerId === authorizer.authorizerId ? (
                      <div
                        data-testid="apigatewayv2-authorizer-delete-confirm"
                        style={fieldRowStyle}
                      >
                        <span style={messageStyle}>Delete this authorizer?</span>
                        <button
                          type="button"
                          data-testid="apigatewayv2-authorizer-delete-confirm-yes"
                          style={buttonStyle}
                          disabled={authorizerDeleteState === 'saving'}
                          onClick={() => handleDeleteAuthorizer(authorizer.authorizerId)}
                        >
                          Confirm delete
                        </button>
                        <button
                          type="button"
                          data-testid="apigatewayv2-authorizer-delete-confirm-no"
                          style={buttonStyle}
                          onClick={() => setPendingDeleteAuthorizerId(null)}
                        >
                          Cancel
                        </button>
                      </div>
                    ) : (
                      <button
                        type="button"
                        data-testid="apigatewayv2-authorizer-delete-toggle"
                        style={buttonStyle}
                        onClick={() => setPendingDeleteAuthorizerId(authorizer.authorizerId)}
                      >
                        Delete authorizer
                      </button>
                    )}
                  </div>
                )}
              </div>
            ))
          : null}
        {authorizerDeleteState === 'error' ? (
          <p data-testid="apigatewayv2-authorizer-delete-error" style={messageStyle}>
            Unable to delete the authorizer.
          </p>
        ) : null}
        {authorizerEditState === 'error' ? (
          <p data-testid="apigatewayv2-authorizer-edit-error" style={messageStyle}>
            Unable to update the authorizer.
          </p>
        ) : null}

        <div data-testid="apigatewayv2-authorizer-create-form" style={formStyle}>
          <Heading as="h4" style={{ fontSize: 13 }}>
            Add authorizer
          </Heading>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-authorizer-new-name">
              Name
            </label>
            <input
              id="apigatewayv2-authorizer-new-name"
              type="text"
              data-testid="apigatewayv2-authorizer-new-name"
              style={inputStyle}
              value={newAuthorizerName}
              onChange={(event) => setNewAuthorizerName(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-authorizer-new-identity">
              Identity source (comma separated)
            </label>
            <input
              id="apigatewayv2-authorizer-new-identity"
              type="text"
              data-testid="apigatewayv2-authorizer-new-identity"
              style={inputStyle}
              value={newAuthorizerIdentitySource}
              onChange={(event) => setNewAuthorizerIdentitySource(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-authorizer-new-issuer">
              JWT issuer
            </label>
            <input
              id="apigatewayv2-authorizer-new-issuer"
              type="text"
              data-testid="apigatewayv2-authorizer-new-issuer"
              style={inputStyle}
              value={newAuthorizerJwtIssuer}
              onChange={(event) => setNewAuthorizerJwtIssuer(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-authorizer-new-audience">
              JWT audience (comma separated)
            </label>
            <input
              id="apigatewayv2-authorizer-new-audience"
              type="text"
              data-testid="apigatewayv2-authorizer-new-audience"
              style={inputStyle}
              value={newAuthorizerJwtAudience}
              onChange={(event) => setNewAuthorizerJwtAudience(event.target.value)}
            />
          </div>
          <button
            type="button"
            data-testid="apigatewayv2-authorizer-new-submit"
            style={buttonStyle}
            disabled={authorizerCreateState === 'saving' || newAuthorizerName.trim() === ''}
            onClick={handleCreateAuthorizer}
          >
            {authorizerCreateState === 'saving' ? 'Adding\u2026' : 'Add authorizer'}
          </button>
          {authorizerCreateState === 'error' ? (
            <p data-testid="apigatewayv2-authorizer-new-error" style={messageStyle}>
              Unable to add the authorizer.
            </p>
          ) : null}
        </div>
      </div>

      <div data-testid="apigatewayv2-stages-section" style={sectionStyle}>
        <Heading as="h3" style={{ fontSize: 15 }}>
          Stages
        </Heading>
        {stagesState.kind === 'loading' ? (
          <p data-testid="apigatewayv2-stages-loading" style={messageStyle}>
            Loading stages&hellip;
          </p>
        ) : null}
        {stagesState.kind === 'error' ? (
          <p data-testid="apigatewayv2-stages-error" style={messageStyle}>
            Unable to load stages.
          </p>
        ) : null}
        {stagesState.kind === 'ready' && stagesState.stages.length === 0 ? (
          <p data-testid="apigatewayv2-stages-empty" style={messageStyle}>
            No stages defined.
          </p>
        ) : null}
        {stagesState.kind === 'ready'
          ? stagesState.stages.map((stage) => (
              <div
                key={stage.stageName}
                data-testid={`apigatewayv2-stage-${stage.stageName}`}
                style={formStyle}
              >
                <span data-testid="apigatewayv2-stage-name" style={valueStyle}>
                  {stage.stageName}
                </span>
                <span style={labelStyle}>
                  Auto deploy: {stage.autoDeploy ? 'Yes' : 'No'}
                </span>
                <span style={labelStyle}>
                  Deployment ID: {stage.deploymentId ?? '\u2014'}
                </span>
                <span data-testid="apigatewayv2-stage-invoke-url" style={labelStyle}>
                  Invoke URL:{' '}
                  {api.apiEndpoint
                    ? `${api.apiEndpoint}/${stage.stageName}`
                    : '\u2014'}
                </span>
                {editingStageName === stage.stageName ? (
                  <div data-testid="apigatewayv2-stage-edit-form" style={fieldRowStyle}>
                    <label style={labelStyle} htmlFor="apigatewayv2-stage-edit-auto-deploy">
                      Auto deploy
                    </label>
                    <input
                      id="apigatewayv2-stage-edit-auto-deploy"
                      type="checkbox"
                      data-testid="apigatewayv2-stage-edit-auto-deploy"
                      checked={editStageAutoDeploy}
                      onChange={(event) => setEditStageAutoDeploy(event.target.checked)}
                    />
                    <label style={labelStyle} htmlFor="apigatewayv2-stage-edit-description">
                      Description
                    </label>
                    <input
                      id="apigatewayv2-stage-edit-description"
                      type="text"
                      data-testid="apigatewayv2-stage-edit-description"
                      style={inputStyle}
                      value={editStageDescription}
                      onChange={(event) => setEditStageDescription(event.target.value)}
                    />
                    <label style={labelStyle} htmlFor="apigatewayv2-stage-edit-burst">
                      Throttling burst limit
                    </label>
                    <input
                      id="apigatewayv2-stage-edit-burst"
                      type="number"
                      data-testid="apigatewayv2-stage-edit-burst"
                      style={inputStyle}
                      value={editStageBurstLimit}
                      onChange={(event) => setEditStageBurstLimit(event.target.value)}
                    />
                    <label style={labelStyle} htmlFor="apigatewayv2-stage-edit-rate">
                      Throttling rate limit
                    </label>
                    <input
                      id="apigatewayv2-stage-edit-rate"
                      type="number"
                      data-testid="apigatewayv2-stage-edit-rate"
                      style={inputStyle}
                      value={editStageRateLimit}
                      onChange={(event) => setEditStageRateLimit(event.target.value)}
                    />
                    <button
                      type="button"
                      data-testid="apigatewayv2-stage-edit-submit"
                      style={buttonStyle}
                      disabled={stageEditState === 'saving'}
                      onClick={() => handleUpdateStage(stage.stageName)}
                    >
                      {stageEditState === 'saving' ? 'Saving\u2026' : 'Save stage'}
                    </button>
                    <button
                      type="button"
                      data-testid="apigatewayv2-stage-edit-cancel"
                      style={buttonStyle}
                      onClick={() => setEditingStageName(null)}
                    >
                      Cancel
                    </button>
                  </div>
                ) : (
                  <div style={fieldRowStyle}>
                    <button
                      type="button"
                      data-testid="apigatewayv2-stage-edit-toggle"
                      style={buttonStyle}
                      onClick={() => handleStartEditStage(stage)}
                    >
                      Edit stage
                    </button>
                    {pendingDeleteStageName === stage.stageName ? (
                      <div
                        data-testid="apigatewayv2-stage-delete-confirm"
                        style={fieldRowStyle}
                      >
                        <span style={messageStyle}>Delete this stage?</span>
                        <button
                          type="button"
                          data-testid="apigatewayv2-stage-delete-confirm-yes"
                          style={buttonStyle}
                          disabled={stageDeleteState === 'saving'}
                          onClick={() => handleDeleteStage(stage.stageName)}
                        >
                          Confirm delete
                        </button>
                        <button
                          type="button"
                          data-testid="apigatewayv2-stage-delete-confirm-no"
                          style={buttonStyle}
                          onClick={() => setPendingDeleteStageName(null)}
                        >
                          Cancel
                        </button>
                      </div>
                    ) : (
                      <button
                        type="button"
                        data-testid="apigatewayv2-stage-delete-toggle"
                        style={buttonStyle}
                        onClick={() => setPendingDeleteStageName(stage.stageName)}
                      >
                        Delete stage
                      </button>
                    )}
                  </div>
                )}
              </div>
            ))
          : null}
        {stageDeleteState === 'error' ? (
          <p data-testid="apigatewayv2-stage-delete-error" style={messageStyle}>
            Unable to delete the stage.
          </p>
        ) : null}
        {stageEditState === 'error' ? (
          <p data-testid="apigatewayv2-stage-edit-error" style={messageStyle}>
            Unable to update the stage.
          </p>
        ) : null}

        <div data-testid="apigatewayv2-stage-create-form" style={formStyle}>
          <Heading as="h4" style={{ fontSize: 13 }}>
            Add stage
          </Heading>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-stage-new-name">
              Stage name
            </label>
            <input
              id="apigatewayv2-stage-new-name"
              type="text"
              data-testid="apigatewayv2-stage-new-name"
              style={inputStyle}
              value={newStageName}
              onChange={(event) => setNewStageName(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-stage-new-auto-deploy">
              Auto deploy
            </label>
            <input
              id="apigatewayv2-stage-new-auto-deploy"
              type="checkbox"
              data-testid="apigatewayv2-stage-new-auto-deploy"
              checked={newStageAutoDeploy}
              onChange={(event) => setNewStageAutoDeploy(event.target.checked)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-stage-new-description">
              Description
            </label>
            <input
              id="apigatewayv2-stage-new-description"
              type="text"
              data-testid="apigatewayv2-stage-new-description"
              style={inputStyle}
              value={newStageDescription}
              onChange={(event) => setNewStageDescription(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-stage-new-burst">
              Throttling burst limit
            </label>
            <input
              id="apigatewayv2-stage-new-burst"
              type="number"
              data-testid="apigatewayv2-stage-new-burst"
              style={inputStyle}
              value={newStageBurstLimit}
              onChange={(event) => setNewStageBurstLimit(event.target.value)}
            />
          </div>
          <div style={fieldRowStyle}>
            <label style={labelStyle} htmlFor="apigatewayv2-stage-new-rate">
              Throttling rate limit
            </label>
            <input
              id="apigatewayv2-stage-new-rate"
              type="number"
              data-testid="apigatewayv2-stage-new-rate"
              style={inputStyle}
              value={newStageRateLimit}
              onChange={(event) => setNewStageRateLimit(event.target.value)}
            />
          </div>
          <button
            type="button"
            data-testid="apigatewayv2-stage-new-submit"
            style={buttonStyle}
            disabled={stageCreateState === 'saving' || newStageName.trim() === ''}
            onClick={handleCreateStage}
          >
            {stageCreateState === 'saving' ? 'Adding\u2026' : 'Add stage'}
          </button>
          {stageCreateState === 'error' ? (
            <p data-testid="apigatewayv2-stage-new-error" style={messageStyle}>
              Unable to add the stage.
            </p>
          ) : null}
        </div>
      </div>

      <div data-testid="apigatewayv2-detail-raw" style={sectionStyle}>
        <RawJsonViewer value={api} title="Raw API" />
      </div>
    </div>
  );
}

export default ApiGatewayV2DetailView;
