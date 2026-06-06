import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import {
  createApiGatewayRestAuthorizer,
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
    </div>
  );
}

export default ApiGatewayAuthorizersSection;
