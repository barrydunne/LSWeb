import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import {
  createApiGatewayRestAuthorizer,
  createApiGatewayRestTokenAuthorizer,
  deleteApiGatewayRestAuthorizer,
  getApiGatewayRestAuthorizer,
  getApiGatewayRestAuthorizers,
  getUserPool,
  getUserPools,
} from '../../api/client';
import type {
  ApiGatewayRestAuthorizerDetailResult,
  ApiGatewayRestAuthorizerItem,
  UserPoolSummaryItem,
} from '../../api/client';

const COGNITO_USER_POOLS = 'COGNITO_USER_POOLS';

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
      authorizers: ApiGatewayRestAuthorizerItem[];
      userPools: UserPoolSummaryItem[];
    }
  | { kind: 'error' };

function emptyToNull(value: string): string | null {
  const trimmed = value.trim();
  return trimmed === '' ? null : trimmed;
}

const IDENTITY_SOURCE_PREFIX = 'method.request.';
const DEFAULT_IDENTITY_SOURCE = 'method.request.header.Authorization';

function isHttpsUrl(value: string): boolean {
  try {
    return new URL(value).protocol === 'https:';
  } catch {
    return false;
  }
}

function isRequestIdentitySource(value: string): boolean {
  return value.startsWith(IDENTITY_SOURCE_PREFIX) && value.length > IDENTITY_SOURCE_PREFIX.length;
}

function validateTokenAuthorizer(
  name: string,
  issuer: string,
  audience: string,
  identitySource: string,
  authorizerUri: string,
): string | null {
  if (name.trim() === '') {
    return 'Name is required.';
  }
  if (!isHttpsUrl(issuer)) {
    return 'Issuer must be an absolute https URL.';
  }
  if (audience.trim() === '') {
    return 'Audience is required.';
  }
  if (!isRequestIdentitySource(identitySource)) {
    return 'Identity source must reference a request value, for example method.request.header.Authorization.';
  }
  if (authorizerUri.trim() === '') {
    return 'Authorizer URI is required.';
  }
  return null;
}

interface ApiGatewayAuthorizersSectionProps {
  restApiId: string;
}

export function ApiGatewayAuthorizersSection({ restApiId }: ApiGatewayAuthorizersSectionProps) {
  const [state, setState] = useState<LoadState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);

  const [newName, setNewName] = useState('');
  const [selectedPoolId, setSelectedPoolId] = useState('');
  const [newIdentitySource, setNewIdentitySource] = useState('');
  const [addError, setAddError] = useState(false);

  const [tokenName, setTokenName] = useState('');
  const [tokenIssuer, setTokenIssuer] = useState('');
  const [tokenAudience, setTokenAudience] = useState('');
  const [tokenIdentitySource, setTokenIdentitySource] = useState(DEFAULT_IDENTITY_SOURCE);
  const [tokenAuthorizerUri, setTokenAuthorizerUri] = useState('');
  const [tokenError, setTokenError] = useState<string | null>(null);

  const [detail, setDetail] = useState<ApiGatewayRestAuthorizerDetailResult | null>(null);

  useEffect(() => {
    const controller = new AbortController();
    setState({ kind: 'loading' });
    Promise.all([
      getApiGatewayRestAuthorizers(restApiId, controller.signal),
      getUserPools(controller.signal),
    ])
      .then(([authorizersResult, poolsResult]) =>
        setState({
          kind: 'ready',
          authorizers: authorizersResult.authorizers,
          userPools: poolsResult.userPools,
        }),
      )
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [restApiId, reloadToken]);

  const refresh = useCallback(() => {
    setReloadToken((token) => token + 1);
  }, []);

  const handleCreate = () => {
    setAddError(false);
    getUserPool(selectedPoolId)
      .then((pool) => {
        if (pool.arn === null) {
          setAddError(true);
          return;
        }
        return createApiGatewayRestAuthorizer(restApiId, {
          name: newName,
          type: COGNITO_USER_POOLS,
          providerARNs: [pool.arn],
          identitySource: emptyToNull(newIdentitySource),
        }).then(() => {
          setNewName('');
          setSelectedPoolId('');
          setNewIdentitySource('');
          refresh();
        });
      })
      .catch(() => setAddError(true));
  };

  const handleCreateToken = () => {
    const validationError = validateTokenAuthorizer(
      tokenName,
      tokenIssuer,
      tokenAudience,
      tokenIdentitySource,
      tokenAuthorizerUri,
    );
    if (validationError !== null) {
      setTokenError(validationError);
      return;
    }
    setTokenError(null);
    createApiGatewayRestTokenAuthorizer(restApiId, {
      name: tokenName.trim(),
      issuer: tokenIssuer.trim(),
      audience: tokenAudience.trim(),
      identitySource: tokenIdentitySource.trim(),
      authorizerUri: tokenAuthorizerUri.trim(),
    })
      .then(() => {
        setTokenName('');
        setTokenIssuer('');
        setTokenAudience('');
        setTokenIdentitySource(DEFAULT_IDENTITY_SOURCE);
        setTokenAuthorizerUri('');
        refresh();
      })
      .catch(() => setTokenError('Unable to add the token authorizer.'));
  };

  const handleDelete = (authorizerId: string) => {
    if (!window.confirm('Delete this authorizer?')) {
      return;
    }
    deleteApiGatewayRestAuthorizer(restApiId, authorizerId)
      .then(refresh)
      .catch(() => setState({ kind: 'error' }));
  };

  const handleView = (authorizerId: string) => {
    setDetail(null);
    getApiGatewayRestAuthorizer(restApiId, authorizerId)
      .then((result) => setDetail(result))
      .catch(() => setDetail(null));
  };

  if (state.kind === 'loading') {
    return (
      <p data-testid="apigateway-authorizers-loading" style={messageStyle}>
        Loading authorizers&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="apigateway-authorizers-error" style={messageStyle}>
        Unable to load authorizers.
      </p>
    );
  }

  const authorizers = state.authorizers;
  const userPools = state.userPools;
  const addDisabled = newName.trim() === '' || selectedPoolId === '';

  return (
    <div data-testid="apigateway-authorizers-section" style={sectionStyle}>
      <span style={headingStyle}>Authorizers</span>

      {authorizers.length === 0 ? (
        <p data-testid="apigateway-authorizers-empty" style={messageStyle}>
          No authorizers found.
        </p>
      ) : null}

      {authorizers.map((authorizer) => (
        <div
          key={authorizer.id}
          data-testid={`apigateway-authorizer-${authorizer.id}`}
          style={rowStyle}
        >
          <div style={inlineStyle}>
            <span data-testid={`apigateway-authorizer-name-${authorizer.id}`} style={valueStyle}>
              {authorizer.name}
            </span>
            <span data-testid={`apigateway-authorizer-type-${authorizer.id}`} style={labelStyle}>
              {authorizer.type}
            </span>
            <button
              type="button"
              data-testid={`apigateway-authorizer-view-${authorizer.id}`}
              style={buttonStyle}
              onClick={() => handleView(authorizer.id)}
            >
              View
            </button>
            <button
              type="button"
              data-testid={`apigateway-authorizer-delete-${authorizer.id}`}
              style={buttonStyle}
              onClick={() => handleDelete(authorizer.id)}
            >
              Delete
            </button>
          </div>
        </div>
      ))}

      {detail !== null ? (
        <div data-testid="apigateway-authorizer-detail" style={rowStyle}>
          <span style={valueStyle}>
            {detail.name} &middot; {detail.type}
          </span>
          <div style={inlineStyle}>
            <span style={labelStyle}>Provider ARNs:</span>
            <span data-testid="apigateway-authorizer-detail-arns" style={valueStyle}>
              {detail.providerARNs.join(', ')}
            </span>
          </div>
          <div style={inlineStyle}>
            <span style={labelStyle}>Identity source:</span>
            <span data-testid="apigateway-authorizer-detail-identity" style={valueStyle}>
              {detail.identitySource ?? '\u2014'}
            </span>
          </div>
          <div style={inlineStyle}>
            <span style={labelStyle}>Auth type:</span>
            <span data-testid="apigateway-authorizer-detail-authtype" style={valueStyle}>
              {detail.authType ?? '\u2014'}
            </span>
          </div>
        </div>
      ) : null}

      <div data-testid="apigateway-authorizer-add-form" style={inlineStyle}>
        <input
          type="text"
          data-testid="apigateway-authorizer-name"
          style={inputStyle}
          placeholder="authorizer name"
          value={newName}
          onChange={(event) => setNewName(event.target.value)}
        />
        <select
          data-testid="apigateway-authorizer-pool"
          style={inputStyle}
          value={selectedPoolId}
          onChange={(event) => setSelectedPoolId(event.target.value)}
        >
          <option value="">(select user pool)</option>
          {userPools.map((pool) => (
            <option key={pool.id} value={pool.id}>
              {pool.name}
            </option>
          ))}
        </select>
        <input
          type="text"
          data-testid="apigateway-authorizer-identity"
          style={inputStyle}
          placeholder="identity source (optional)"
          value={newIdentitySource}
          onChange={(event) => setNewIdentitySource(event.target.value)}
        />
        <button
          type="button"
          data-testid="apigateway-authorizer-add"
          style={buttonStyle}
          disabled={addDisabled}
          onClick={handleCreate}
        >
          Add authorizer
        </button>
      </div>

      {addError ? (
        <p data-testid="apigateway-authorizer-add-error" style={messageStyle}>
          Unable to add the authorizer.
        </p>
      ) : null}

      <span style={headingStyle}>OAuth / JWT token authorizer</span>
      <p data-testid="apigateway-token-authorizer-hint" style={labelStyle}>
        Guided setup for validating OIDC bearer tokens. Issuer and audience are validated here and
        enforced by the authorizer function the URI points to.
      </p>
      <div data-testid="apigateway-token-authorizer-form" style={inlineStyle}>
        <input
          type="text"
          data-testid="apigateway-token-authorizer-name"
          style={inputStyle}
          placeholder="authorizer name"
          value={tokenName}
          onChange={(event) => setTokenName(event.target.value)}
        />
        <input
          type="text"
          data-testid="apigateway-token-authorizer-issuer"
          style={inputStyle}
          placeholder="issuer (https://...)"
          value={tokenIssuer}
          onChange={(event) => setTokenIssuer(event.target.value)}
        />
        <input
          type="text"
          data-testid="apigateway-token-authorizer-audience"
          style={inputStyle}
          placeholder="audience"
          value={tokenAudience}
          onChange={(event) => setTokenAudience(event.target.value)}
        />
        <input
          type="text"
          data-testid="apigateway-token-authorizer-identity"
          style={inputStyle}
          placeholder="identity source"
          value={tokenIdentitySource}
          onChange={(event) => setTokenIdentitySource(event.target.value)}
        />
        <input
          type="text"
          data-testid="apigateway-token-authorizer-uri"
          style={inputStyle}
          placeholder="authorizer function URI"
          value={tokenAuthorizerUri}
          onChange={(event) => setTokenAuthorizerUri(event.target.value)}
        />
        <button
          type="button"
          data-testid="apigateway-token-authorizer-add"
          style={buttonStyle}
          onClick={handleCreateToken}
        >
          Add token authorizer
        </button>
      </div>

      {tokenError !== null ? (
        <p data-testid="apigateway-token-authorizer-error" style={messageStyle}>
          {tokenError}
        </p>
      ) : null}
    </div>
  );
}

export default ApiGatewayAuthorizersSection;
