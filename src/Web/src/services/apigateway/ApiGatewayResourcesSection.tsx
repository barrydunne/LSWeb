import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import {
  createApiGatewayRestResource,
  deleteApiGatewayRestMethod,
  deleteApiGatewayRestResource,
  getApiGatewayRestMethod,
  getApiGatewayRestResources,
  putApiGatewayRestMethod,
} from '../../api/client';
import type {
  ApiGatewayRestMethodDetailResult,
  ApiGatewayRestResourceItem,
} from '../../api/client';

const HTTP_METHODS = ['GET', 'POST', 'PUT', 'DELETE', 'PATCH', 'HEAD', 'OPTIONS', 'ANY'];
const AUTHORIZATION_TYPES = ['NONE', 'AWS_IAM', 'CUSTOM', 'COGNITO_USER_POOLS'];

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

const resourceRowStyle: CSSProperties = {
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
  | { kind: 'ready'; resources: ApiGatewayRestResourceItem[] }
  | { kind: 'error' };

function rootResourceId(resources: ApiGatewayRestResourceItem[]): string {
  const root = resources.find((resource) => resource.parentId === null);
  return root?.id ?? '';
}

interface ApiGatewayResourcesSectionProps {
  restApiId: string;
}

export function ApiGatewayResourcesSection({ restApiId }: ApiGatewayResourcesSectionProps) {
  const [state, setState] = useState<LoadState>({ kind: 'loading' });
  const [reloadToken, setReloadToken] = useState(0);

  const [newParentId, setNewParentId] = useState('');
  const [newPathPart, setNewPathPart] = useState('');
  const [addError, setAddError] = useState(false);

  const [methodResourceId, setMethodResourceId] = useState('');
  const [newHttpMethod, setNewHttpMethod] = useState('GET');
  const [newAuthType, setNewAuthType] = useState('NONE');
  const [methodError, setMethodError] = useState(false);

  const [methodDetail, setMethodDetail] = useState<ApiGatewayRestMethodDetailResult | null>(null);

  useEffect(() => {
    const controller = new AbortController();
    setState({ kind: 'loading' });
    getApiGatewayRestResources(restApiId, controller.signal)
      .then((result) => setState({ kind: 'ready', resources: result.resources }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [restApiId, reloadToken]);

  const refresh = useCallback(() => {
    setReloadToken((token) => token + 1);
  }, []);

  const handleAddResource = (resources: ApiGatewayRestResourceItem[]) => {
    setAddError(false);
    const parentId = newParentId === '' ? rootResourceId(resources) : newParentId;
    createApiGatewayRestResource(restApiId, { parentId, pathPart: newPathPart })
      .then(() => {
        setNewPathPart('');
        setNewParentId('');
        refresh();
      })
      .catch(() => setAddError(true));
  };

  const handleDeleteResource = (resourceId: string) => {
    if (!window.confirm('Delete this resource?')) {
      return;
    }
    deleteApiGatewayRestResource(restApiId, resourceId)
      .then(refresh)
      .catch(() => setState({ kind: 'error' }));
  };

  const handleAddMethod = (resourceId: string) => {
    setMethodError(false);
    putApiGatewayRestMethod(restApiId, resourceId, newHttpMethod, {
      authorizationType: newAuthType,
      authorizerId: null,
      apiKeyRequired: false,
      authorizationScopes: [],
    })
      .then(refresh)
      .catch(() => setMethodError(true));
  };

  const handleDeleteMethod = (resourceId: string, httpMethod: string) => {
    if (!window.confirm(`Delete the ${httpMethod} method?`)) {
      return;
    }
    deleteApiGatewayRestMethod(restApiId, resourceId, httpMethod)
      .then(refresh)
      .catch(() => setState({ kind: 'error' }));
  };

  const handleViewMethod = (resourceId: string, httpMethod: string) => {
    setMethodDetail(null);
    getApiGatewayRestMethod(restApiId, resourceId, httpMethod)
      .then((detail) => setMethodDetail(detail))
      .catch(() => setMethodDetail(null));
  };

  if (state.kind === 'loading') {
    return (
      <p data-testid="apigateway-resources-loading" style={messageStyle}>
        Loading resources&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="apigateway-resources-error" style={messageStyle}>
        Unable to load resources.
      </p>
    );
  }

  const resources = state.resources;

  return (
    <div data-testid="apigateway-resources-section" style={sectionStyle}>
      <span style={headingStyle}>Resources</span>

      {resources.length === 0 ? (
        <p data-testid="apigateway-resources-empty" style={messageStyle}>
          No resources found.
        </p>
      ) : null}

      {resources.map((resource) => (
        <div
          key={resource.id}
          data-testid={`apigateway-resource-${resource.id}`}
          style={resourceRowStyle}
        >
          <div style={inlineStyle}>
            <span data-testid={`apigateway-resource-path-${resource.id}`} style={valueStyle}>
              {resource.path}
            </span>
            {resource.parentId !== null ? (
              <button
                type="button"
                data-testid={`apigateway-resource-delete-${resource.id}`}
                style={buttonStyle}
                onClick={() => handleDeleteResource(resource.id)}
              >
                Delete resource
              </button>
            ) : null}
          </div>

          <div style={inlineStyle}>
            <span style={labelStyle}>Methods:</span>
            {resource.resourceMethods.length === 0 ? (
              <span
                data-testid={`apigateway-resource-no-methods-${resource.id}`}
                style={valueStyle}
              >
                {'\u2014'}
              </span>
            ) : (
              resource.resourceMethods.map((httpMethod) => (
                <span key={httpMethod} style={inlineStyle}>
                  <button
                    type="button"
                    data-testid={`apigateway-method-view-${resource.id}-${httpMethod}`}
                    style={buttonStyle}
                    onClick={() => handleViewMethod(resource.id, httpMethod)}
                  >
                    {httpMethod}
                  </button>
                  <button
                    type="button"
                    data-testid={`apigateway-method-delete-${resource.id}-${httpMethod}`}
                    style={buttonStyle}
                    onClick={() => handleDeleteMethod(resource.id, httpMethod)}
                  >
                    {'\u2715'}
                  </button>
                </span>
              ))
            )}
          </div>

          <div style={inlineStyle}>
            <select
              data-testid={`apigateway-method-http-${resource.id}`}
              style={inputStyle}
              value={methodResourceId === resource.id ? newHttpMethod : 'GET'}
              onChange={(event) => {
                setMethodResourceId(resource.id);
                setNewHttpMethod(event.target.value);
              }}
            >
              {HTTP_METHODS.map((method) => (
                <option key={method} value={method}>
                  {method}
                </option>
              ))}
            </select>
            <select
              data-testid={`apigateway-method-auth-${resource.id}`}
              style={inputStyle}
              value={methodResourceId === resource.id ? newAuthType : 'NONE'}
              onChange={(event) => {
                setMethodResourceId(resource.id);
                setNewAuthType(event.target.value);
              }}
            >
              {AUTHORIZATION_TYPES.map((authType) => (
                <option key={authType} value={authType}>
                  {authType}
                </option>
              ))}
            </select>
            <button
              type="button"
              data-testid={`apigateway-method-add-${resource.id}`}
              style={buttonStyle}
              onClick={() => handleAddMethod(resource.id)}
            >
              Add method
            </button>
          </div>
        </div>
      ))}

      {methodDetail !== null ? (
        <div data-testid="apigateway-method-detail" style={resourceRowStyle}>
          <span style={valueStyle}>
            {methodDetail.httpMethod} &middot; {methodDetail.authorizationType}
          </span>
        </div>
      ) : null}

      {methodError ? (
        <p data-testid="apigateway-method-error" style={messageStyle}>
          Unable to save the method.
        </p>
      ) : null}

      <div data-testid="apigateway-resource-add-form" style={inlineStyle}>
        <select
          data-testid="apigateway-resource-parent"
          style={inputStyle}
          value={newParentId}
          onChange={(event) => setNewParentId(event.target.value)}
        >
          <option value="">(root)</option>
          {resources.map((resource) => (
            <option key={resource.id} value={resource.id}>
              {resource.path}
            </option>
          ))}
        </select>
        <input
          type="text"
          data-testid="apigateway-resource-path-part"
          style={inputStyle}
          placeholder="path part"
          value={newPathPart}
          onChange={(event) => setNewPathPart(event.target.value)}
        />
        <button
          type="button"
          data-testid="apigateway-resource-add"
          style={buttonStyle}
          disabled={newPathPart.trim() === ''}
          onClick={() => handleAddResource(resources)}
        >
          Add resource
        </button>
      </div>

      {addError ? (
        <p data-testid="apigateway-resource-add-error" style={messageStyle}>
          Unable to add the resource.
        </p>
      ) : null}
    </div>
  );
}

export default ApiGatewayResourcesSection;
