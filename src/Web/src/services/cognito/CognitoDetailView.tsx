import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading } from '@primer/react';
import {
  createUserPoolClient,
  deleteUserPoolClient,
  getUserPool,
  getUserPoolClient,
  getUserPoolClients,
  updateUserPoolClient,
} from '../../api/client';
import type {
  UserPoolClientDetailResult,
  UserPoolClientSummaryItem,
  UserPoolDetailResult,
} from '../../api/client';
import type { ServiceDetailViewProps } from '../serviceViewRegistry';
import { RawJsonViewer } from '../../components/RawJsonViewer';
import { ConfirmationHost } from '../../components/ConfirmationHost';

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

const clientsSectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
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

const checkboxRowStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  gap: 6,
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

const clientRowStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  gap: 12,
  padding: '6px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const clientActionsStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  gap: 8,
};

type LoadState =
  | { kind: 'loading' }
  | { kind: 'ready'; userPool: UserPoolDetailResult }
  | { kind: 'error' };

type ClientsState =
  | { kind: 'loading' }
  | { kind: 'ready'; clients: UserPoolClientSummaryItem[] }
  | { kind: 'error' };

type SaveState = 'idle' | 'saving' | 'error';

const MASKED_SECRET = '\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022';

function formatAttributes(attributes: string[]): string {
  return attributes.length === 0 ? '—' : attributes.join(', ');
}

function parseList(value: string): string[] {
  return value
    .split(',')
    .map((entry) => entry.trim())
    .filter((entry) => entry !== '');
}

export function CognitoDetailView({ resourceId }: ServiceDetailViewProps) {
  const [state, setState] = useState<LoadState>({ kind: 'loading' });
  const [clientsState, setClientsState] = useState<ClientsState>({ kind: 'loading' });
  const [clientsReloadToken, setClientsReloadToken] = useState(0);

  const [showCreate, setShowCreate] = useState(false);
  const [createName, setCreateName] = useState('');
  const [createGenerateSecret, setCreateGenerateSecret] = useState(false);
  const [createExplicitAuthFlows, setCreateExplicitAuthFlows] = useState('');
  const [createAllowedOAuthFlows, setCreateAllowedOAuthFlows] = useState('');
  const [createAllowedOAuthScopes, setCreateAllowedOAuthScopes] = useState('');
  const [createCallbackUrls, setCreateCallbackUrls] = useState('');
  const [createOAuthUserPoolClient, setCreateOAuthUserPoolClient] = useState(false);
  const [createState, setCreateState] = useState<SaveState>('idle');

  const [selectedClient, setSelectedClient] = useState<UserPoolClientDetailResult | null>(null);
  const [selectedState, setSelectedState] = useState<'idle' | 'loading' | 'error'>('idle');
  const [secretRevealed, setSecretRevealed] = useState(false);

  const [editing, setEditing] = useState(false);
  const [editName, setEditName] = useState('');
  const [editExplicitAuthFlows, setEditExplicitAuthFlows] = useState('');
  const [editAllowedOAuthFlows, setEditAllowedOAuthFlows] = useState('');
  const [editAllowedOAuthScopes, setEditAllowedOAuthScopes] = useState('');
  const [editCallbackUrls, setEditCallbackUrls] = useState('');
  const [editOAuthUserPoolClient, setEditOAuthUserPoolClient] = useState(false);
  const [editState, setEditState] = useState<SaveState>('idle');

  useEffect(() => {
    const controller = new AbortController();
    getUserPool(resourceId, controller.signal)
      .then((userPool) => setState({ kind: 'ready', userPool }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [resourceId]);

  useEffect(() => {
    const controller = new AbortController();
    setClientsState({ kind: 'loading' });
    getUserPoolClients(resourceId, controller.signal)
      .then((result) => setClientsState({ kind: 'ready', clients: result.clients }))
      .catch(() => setClientsState({ kind: 'error' }));
    return () => controller.abort();
  }, [resourceId, clientsReloadToken]);

  const refreshClients = useCallback(() => {
    setClientsReloadToken((token) => token + 1);
  }, []);

  const handleCreate = () => {
    setCreateState('saving');
    createUserPoolClient(resourceId, {
      clientName: createName,
      generateSecret: createGenerateSecret,
      explicitAuthFlows: parseList(createExplicitAuthFlows),
      allowedOAuthFlows: parseList(createAllowedOAuthFlows),
      allowedOAuthScopes: parseList(createAllowedOAuthScopes),
      callbackURLs: parseList(createCallbackUrls),
      allowedOAuthFlowsUserPoolClient: createOAuthUserPoolClient,
    })
      .then(() => {
        setCreateState('idle');
        setCreateName('');
        setCreateGenerateSecret(false);
        setCreateExplicitAuthFlows('');
        setCreateAllowedOAuthFlows('');
        setCreateAllowedOAuthScopes('');
        setCreateCallbackUrls('');
        setCreateOAuthUserPoolClient(false);
        setShowCreate(false);
        refreshClients();
      })
      .catch(() => setCreateState('error'));
  };

  const handleSelect = useCallback(
    (clientId: string) => {
      setSelectedState('loading');
      setSecretRevealed(false);
      setEditing(false);
      getUserPoolClient(resourceId, clientId)
        .then((client) => {
          setSelectedClient(client);
          setSelectedState('idle');
        })
        .catch(() => setSelectedState('error'));
    },
    [resourceId],
  );

  const handleStartEdit = (client: UserPoolClientDetailResult) => {
    setEditName(client.clientName);
    setEditExplicitAuthFlows(client.explicitAuthFlows.join(', '));
    setEditAllowedOAuthFlows(client.allowedOAuthFlows.join(', '));
    setEditAllowedOAuthScopes(client.allowedOAuthScopes.join(', '));
    setEditCallbackUrls(client.callbackURLs.join(', '));
    setEditOAuthUserPoolClient(client.allowedOAuthFlowsUserPoolClient);
    setEditState('idle');
    setEditing(true);
  };

  const handleUpdate = (clientId: string) => {
    setEditState('saving');
    updateUserPoolClient(resourceId, clientId, {
      clientName: editName,
      explicitAuthFlows: parseList(editExplicitAuthFlows),
      allowedOAuthFlows: parseList(editAllowedOAuthFlows),
      allowedOAuthScopes: parseList(editAllowedOAuthScopes),
      callbackURLs: parseList(editCallbackUrls),
      allowedOAuthFlowsUserPoolClient: editOAuthUserPoolClient,
    })
      .then(() => {
        setEditing(false);
        setEditState('idle');
        handleSelect(clientId);
        refreshClients();
      })
      .catch(() => setEditState('error'));
  };

  const handleDelete = useCallback(
    (clientId: string) => {
      deleteUserPoolClient(resourceId, clientId)
        .then(() => {
          setSelectedClient((current) =>
            current && current.clientId === clientId ? null : current,
          );
          refreshClients();
        })
        .catch(() => setClientsState({ kind: 'error' }));
    },
    [refreshClients, resourceId],
  );

  if (state.kind === 'loading') {
    return (
      <p data-testid="cognito-detail-loading" style={messageStyle}>
        Loading user pool&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="cognito-detail-error" style={messageStyle}>
        Unable to load the user pool.
      </p>
    );
  }

  const userPool = state.userPool;

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
      <div data-testid="cognito-detail-view" style={containerStyle}>
        <Heading as="h2" data-testid="cognito-detail-name" style={{ fontSize: 18 }}>
          {userPool.name}
        </Heading>
        <div style={rowStyle}>
          <span style={labelStyle}>Pool ID</span>
          <span data-testid="cognito-detail-id" style={valueStyle}>
            {userPool.id}
          </span>
        </div>
        <div style={rowStyle}>
          <span style={labelStyle}>ARN</span>
          <span data-testid="cognito-detail-arn" style={valueStyle}>
            {userPool.arn ?? '—'}
          </span>
        </div>
        <div style={rowStyle}>
          <span style={labelStyle}>MFA configuration</span>
          <span data-testid="cognito-detail-mfa" style={valueStyle}>
            {userPool.mfaConfiguration ?? '—'}
          </span>
        </div>
        <div style={rowStyle}>
          <span style={labelStyle}>Estimated users</span>
          <span data-testid="cognito-detail-users" style={valueStyle}>
            {userPool.estimatedNumberOfUsers ?? '—'}
          </span>
        </div>
        <div style={rowStyle}>
          <span style={labelStyle}>Username attributes</span>
          <span data-testid="cognito-detail-username-attributes" style={valueStyle}>
            {formatAttributes(userPool.usernameAttributes)}
          </span>
        </div>
        <div style={rowStyle}>
          <span style={labelStyle}>Auto-verified attributes</span>
          <span data-testid="cognito-detail-auto-verified-attributes" style={valueStyle}>
            {formatAttributes(userPool.autoVerifiedAttributes)}
          </span>
        </div>
        <div style={rowStyle}>
          <span style={labelStyle}>Created</span>
          <span data-testid="cognito-detail-created" style={valueStyle}>
            {userPool.creationDate ?? '—'}
          </span>
        </div>
        <div style={rowStyle}>
          <span style={labelStyle}>Last modified</span>
          <span data-testid="cognito-detail-modified" style={valueStyle}>
            {userPool.lastModifiedDate ?? '—'}
          </span>
        </div>
        <div data-testid="cognito-detail-raw" style={sectionStyle}>
          <RawJsonViewer value={userPool} title="Raw user pool" />
        </div>
      </div>

      <div data-testid="cognito-clients-section" style={clientsSectionStyle}>
        <Heading as="h3" style={{ fontSize: 16 }}>
          App clients
        </Heading>
        <button
          type="button"
          data-testid="cognito-client-create-toggle"
          style={buttonStyle}
          onClick={() => setShowCreate((current) => !current)}
        >
          {showCreate ? 'Cancel' : 'Create app client'}
        </button>
        {showCreate ? (
          <div data-testid="cognito-client-create-form" style={formStyle}>
            <div style={fieldRowStyle}>
              <label style={labelStyle} htmlFor="cognito-client-create-name">
                Client name
              </label>
              <input
                id="cognito-client-create-name"
                type="text"
                data-testid="cognito-client-create-name"
                style={inputStyle}
                value={createName}
                onChange={(event) => setCreateName(event.target.value)}
              />
            </div>
            <label style={checkboxRowStyle}>
              <input
                type="checkbox"
                data-testid="cognito-client-create-generate-secret"
                checked={createGenerateSecret}
                onChange={(event) => setCreateGenerateSecret(event.target.checked)}
              />
              <span style={labelStyle}>Generate secret</span>
            </label>
            <div style={fieldRowStyle}>
              <label style={labelStyle} htmlFor="cognito-client-create-auth-flows">
                Explicit auth flows (comma separated)
              </label>
              <input
                id="cognito-client-create-auth-flows"
                type="text"
                data-testid="cognito-client-create-auth-flows"
                style={inputStyle}
                value={createExplicitAuthFlows}
                onChange={(event) => setCreateExplicitAuthFlows(event.target.value)}
              />
            </div>
            <div style={fieldRowStyle}>
              <label style={labelStyle} htmlFor="cognito-client-create-oauth-flows">
                Allowed OAuth flows (comma separated)
              </label>
              <input
                id="cognito-client-create-oauth-flows"
                type="text"
                data-testid="cognito-client-create-oauth-flows"
                style={inputStyle}
                value={createAllowedOAuthFlows}
                onChange={(event) => setCreateAllowedOAuthFlows(event.target.value)}
              />
            </div>
            <div style={fieldRowStyle}>
              <label style={labelStyle} htmlFor="cognito-client-create-oauth-scopes">
                Allowed OAuth scopes (comma separated)
              </label>
              <input
                id="cognito-client-create-oauth-scopes"
                type="text"
                data-testid="cognito-client-create-oauth-scopes"
                style={inputStyle}
                value={createAllowedOAuthScopes}
                onChange={(event) => setCreateAllowedOAuthScopes(event.target.value)}
              />
            </div>
            <div style={fieldRowStyle}>
              <label style={labelStyle} htmlFor="cognito-client-create-callbacks">
                Callback URLs (comma separated)
              </label>
              <input
                id="cognito-client-create-callbacks"
                type="text"
                data-testid="cognito-client-create-callbacks"
                style={inputStyle}
                value={createCallbackUrls}
                onChange={(event) => setCreateCallbackUrls(event.target.value)}
              />
            </div>
            <label style={checkboxRowStyle}>
              <input
                type="checkbox"
                data-testid="cognito-client-create-oauth-user-pool-client"
                checked={createOAuthUserPoolClient}
                onChange={(event) => setCreateOAuthUserPoolClient(event.target.checked)}
              />
              <span style={labelStyle}>Allowed OAuth flows user pool client</span>
            </label>
            <button
              type="button"
              data-testid="cognito-client-create-submit"
              style={buttonStyle}
              disabled={createState === 'saving'}
              onClick={handleCreate}
            >
              {createState === 'saving' ? 'Creating\u2026' : 'Create'}
            </button>
            {createState === 'error' ? (
              <p data-testid="cognito-client-create-error" style={messageStyle}>
                Unable to create the app client.
              </p>
            ) : null}
          </div>
        ) : null}

        {clientsState.kind === 'loading' ? (
          <p data-testid="cognito-clients-loading" style={messageStyle}>
            Loading app clients&hellip;
          </p>
        ) : null}
        {clientsState.kind === 'error' ? (
          <p data-testid="cognito-clients-error" style={messageStyle}>
            Unable to load app clients.
          </p>
        ) : null}
        {clientsState.kind === 'ready' && clientsState.clients.length === 0 ? (
          <p data-testid="cognito-clients-empty" style={messageStyle}>
            No app clients found for this user pool.
          </p>
        ) : null}
        {clientsState.kind === 'ready' && clientsState.clients.length > 0 ? (
          <div data-testid="cognito-clients-list" style={sectionStyle}>
            {clientsState.clients.map((client) => (
              <div key={client.clientId} data-testid="cognito-client-row" style={clientRowStyle}>
                <span style={valueStyle}>
                  {client.clientName} ({client.clientId})
                </span>
                <div style={clientActionsStyle}>
                  <button
                    type="button"
                    data-testid="cognito-client-view"
                    style={buttonStyle}
                    onClick={() => handleSelect(client.clientId)}
                  >
                    View
                  </button>
                  <ConfirmationHost
                    actionLabel="Delete"
                    prompt={`Delete ${client.clientName}?`}
                    confirmLabel="Confirm"
                    onConfirm={() => handleDelete(client.clientId)}
                  />
                </div>
              </div>
            ))}
          </div>
        ) : null}

        {selectedState === 'loading' ? (
          <p data-testid="cognito-client-detail-loading" style={messageStyle}>
            Loading app client&hellip;
          </p>
        ) : null}
        {selectedState === 'error' ? (
          <p data-testid="cognito-client-detail-error" style={messageStyle}>
            Unable to load the app client.
          </p>
        ) : null}
        {selectedState === 'idle' && selectedClient !== null ? (
          <div data-testid="cognito-client-detail" style={formStyle}>
            <div style={rowStyle}>
              <span style={labelStyle}>Client ID</span>
              <span data-testid="cognito-client-detail-id" style={valueStyle}>
                {selectedClient.clientId}
              </span>
            </div>
            <div style={rowStyle}>
              <span style={labelStyle}>Client name</span>
              <span data-testid="cognito-client-detail-name" style={valueStyle}>
                {selectedClient.clientName}
              </span>
            </div>
            <div style={rowStyle}>
              <span style={labelStyle}>Client secret</span>
              <div style={checkboxRowStyle}>
                <span data-testid="cognito-client-detail-secret" style={valueStyle}>
                  {selectedClient.clientSecret === null
                    ? '—'
                    : secretRevealed
                      ? selectedClient.clientSecret
                      : MASKED_SECRET}
                </span>
                {selectedClient.clientSecret === null ? null : (
                  <button
                    type="button"
                    data-testid="cognito-client-detail-secret-toggle"
                    style={buttonStyle}
                    onClick={() => setSecretRevealed((current) => !current)}
                  >
                    {secretRevealed ? 'Hide' : 'Reveal'}
                  </button>
                )}
              </div>
            </div>
            <div style={rowStyle}>
              <span style={labelStyle}>Explicit auth flows</span>
              <span data-testid="cognito-client-detail-auth-flows" style={valueStyle}>
                {formatAttributes(selectedClient.explicitAuthFlows)}
              </span>
            </div>
            <div style={rowStyle}>
              <span style={labelStyle}>Allowed OAuth flows</span>
              <span data-testid="cognito-client-detail-oauth-flows" style={valueStyle}>
                {formatAttributes(selectedClient.allowedOAuthFlows)}
              </span>
            </div>
            <div style={rowStyle}>
              <span style={labelStyle}>Allowed OAuth scopes</span>
              <span data-testid="cognito-client-detail-oauth-scopes" style={valueStyle}>
                {formatAttributes(selectedClient.allowedOAuthScopes)}
              </span>
            </div>
            <div style={rowStyle}>
              <span style={labelStyle}>Callback URLs</span>
              <span data-testid="cognito-client-detail-callbacks" style={valueStyle}>
                {formatAttributes(selectedClient.callbackURLs)}
              </span>
            </div>
            <button
              type="button"
              data-testid="cognito-client-edit-toggle"
              style={buttonStyle}
              onClick={() => handleStartEdit(selectedClient)}
            >
              Edit
            </button>
            {editing ? (
              <div data-testid="cognito-client-edit-form" style={formStyle}>
                <div style={fieldRowStyle}>
                  <label style={labelStyle} htmlFor="cognito-client-edit-name">
                    Client name
                  </label>
                  <input
                    id="cognito-client-edit-name"
                    type="text"
                    data-testid="cognito-client-edit-name"
                    style={inputStyle}
                    value={editName}
                    onChange={(event) => setEditName(event.target.value)}
                  />
                </div>
                <div style={fieldRowStyle}>
                  <label style={labelStyle} htmlFor="cognito-client-edit-auth-flows">
                    Explicit auth flows (comma separated)
                  </label>
                  <input
                    id="cognito-client-edit-auth-flows"
                    type="text"
                    data-testid="cognito-client-edit-auth-flows"
                    style={inputStyle}
                    value={editExplicitAuthFlows}
                    onChange={(event) => setEditExplicitAuthFlows(event.target.value)}
                  />
                </div>
                <div style={fieldRowStyle}>
                  <label style={labelStyle} htmlFor="cognito-client-edit-oauth-flows">
                    Allowed OAuth flows (comma separated)
                  </label>
                  <input
                    id="cognito-client-edit-oauth-flows"
                    type="text"
                    data-testid="cognito-client-edit-oauth-flows"
                    style={inputStyle}
                    value={editAllowedOAuthFlows}
                    onChange={(event) => setEditAllowedOAuthFlows(event.target.value)}
                  />
                </div>
                <div style={fieldRowStyle}>
                  <label style={labelStyle} htmlFor="cognito-client-edit-oauth-scopes">
                    Allowed OAuth scopes (comma separated)
                  </label>
                  <input
                    id="cognito-client-edit-oauth-scopes"
                    type="text"
                    data-testid="cognito-client-edit-oauth-scopes"
                    style={inputStyle}
                    value={editAllowedOAuthScopes}
                    onChange={(event) => setEditAllowedOAuthScopes(event.target.value)}
                  />
                </div>
                <div style={fieldRowStyle}>
                  <label style={labelStyle} htmlFor="cognito-client-edit-callbacks">
                    Callback URLs (comma separated)
                  </label>
                  <input
                    id="cognito-client-edit-callbacks"
                    type="text"
                    data-testid="cognito-client-edit-callbacks"
                    style={inputStyle}
                    value={editCallbackUrls}
                    onChange={(event) => setEditCallbackUrls(event.target.value)}
                  />
                </div>
                <label style={checkboxRowStyle}>
                  <input
                    type="checkbox"
                    data-testid="cognito-client-edit-oauth-user-pool-client"
                    checked={editOAuthUserPoolClient}
                    onChange={(event) => setEditOAuthUserPoolClient(event.target.checked)}
                  />
                  <span style={labelStyle}>Allowed OAuth flows user pool client</span>
                </label>
                <button
                  type="button"
                  data-testid="cognito-client-edit-submit"
                  style={buttonStyle}
                  disabled={editState === 'saving'}
                  onClick={() => handleUpdate(selectedClient.clientId)}
                >
                  {editState === 'saving' ? 'Saving\u2026' : 'Save'}
                </button>
                {editState === 'error' ? (
                  <p data-testid="cognito-client-edit-error" style={messageStyle}>
                    Unable to update the app client.
                  </p>
                ) : null}
              </div>
            ) : null}
          </div>
        ) : null}
      </div>
    </div>
  );
}

export default CognitoDetailView;
