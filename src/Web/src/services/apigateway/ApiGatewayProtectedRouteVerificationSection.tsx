import { useEffect, useMemo, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading } from '@primer/react';
import {
  getApiGatewayRestResources,
  getApiGatewayRestMethod,
  testInvokeApiGatewayRestMethod,
} from '../../api/client';
import type {
  ApiGatewayRestResourceItem,
  ApiGatewayRestMethodTestInvokeResult,
} from '../../api/client';

const sectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};

const sectionHeadingStyle: CSSProperties = { fontSize: 14, fontWeight: 600 };
const messageStyle: CSSProperties = { fontSize: 14 };
const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };

const formStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
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

const resultsContainerStyle: CSSProperties = {
  display: 'grid',
  gridTemplateColumns: '1fr 1fr',
  gap: 12,
};

const resultCardStyle: CSSProperties = {
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#11161d',
};

const passStyle: CSSProperties = {
  ...resultCardStyle,
  borderColor: '#238636',
  background: '#0d3920',
};

const failStyle: CSSProperties = {
  ...resultCardStyle,
  borderColor: '#da3633',
  background: '#3d2626',
};

const statusBadgeStyle = (statusCode: number): CSSProperties => ({
  display: 'inline-block',
  padding: '2px 6px',
  borderRadius: 3,
  fontSize: 12,
  fontWeight: 600,
  background: statusCode < 300 ? '#0d3920' : statusCode < 400 ? '#3d3d1a' : '#3d2626',
  color: statusCode < 300 ? '#3fb950' : statusCode < 400 ? '#d4a574' : '#f85149',
});

type LoadState =
  | { kind: 'loading' }
  | { kind: 'ready'; resources: ApiGatewayRestResourceItem[]; protectedMethods: ProtectedMethod[] }
  | { kind: 'error' };

interface ProtectedMethod {
  resourceId: string;
  resourcePath: string;
  httpMethod: string;
  authorizationType: string;
  authorizerId: string | null;
}

interface VerificationResult {
  unauthorized: ApiGatewayRestMethodTestInvokeResult | null;
  authorized: ApiGatewayRestMethodTestInvokeResult | null;
  unauthorizedError: boolean;
  authorizedError: boolean;
}

export function ApiGatewayProtectedRouteVerificationSection({ restApiId }: { restApiId: string }) {
  const [state, setState] = useState<LoadState>({ kind: 'loading' });
  const [selectedIndex, setSelectedIndex] = useState(0);
  const [verifying, setVerifying] = useState(false);
  const [verificationResult, setVerificationResult] = useState<VerificationResult | null>(null);
  const [authorizationToken, setAuthorizationToken] = useState('Bearer mock-token-12345');

  useEffect(() => {
    const controller = new AbortController();
    setState({ kind: 'loading' });
    setSelectedIndex(0);

    (async () => {
      try {
        const response = await getApiGatewayRestResources(restApiId, controller.signal);
        const resources = response.resources;

        // Discover protected methods
        const protectedMethods: ProtectedMethod[] = [];
        for (const resource of resources) {
          for (const method of resource.resourceMethods) {
            try {
              const methodDetail = await getApiGatewayRestMethod(
                restApiId,
                resource.id,
                method,
                controller.signal
              );
              if (methodDetail.authorizationType && methodDetail.authorizationType !== 'NONE') {
                protectedMethods.push({
                  resourceId: resource.id,
                  resourcePath: resource.path || '/',
                  httpMethod: method,
                  authorizationType: methodDetail.authorizationType,
                  authorizerId: methodDetail.authorizerId,
                });
              }
            } catch {
              // Skip methods we can't get details for
            }
          }
        }

        setState({ kind: 'ready', resources, protectedMethods });
      } catch {
        setState({ kind: 'error' });
      }
    })();

    return () => controller.abort();
  }, [restApiId]);

  const selectedMethod = useMemo(() => {
    if (state.kind !== 'ready') {
      return null;
    }
    return state.protectedMethods[selectedIndex] ?? null;
  }, [state, selectedIndex]);

  const handleVerify = async () => {
    const method = selectedMethod!;

    setVerifying(true);
    setVerificationResult(null);

    try {
      // Test 1: Without authorization (should fail with 401/403)
      let unauthorizedResult: ApiGatewayRestMethodTestInvokeResult | null = null;
      let unauthorizedError = false;
      try {
        unauthorizedResult = await testInvokeApiGatewayRestMethod(
          restApiId,
          method.resourceId,
          method.httpMethod,
          {
            pathWithQueryString: method.resourcePath,
            headers: {},
            queryStringParameters: {},
            body: null,
            stageVariables: {},
          }
        );
      } catch {
        unauthorizedError = true;
      }

      // Test 2: With authorization header (test if it's accepted)
      let authorizedResult: ApiGatewayRestMethodTestInvokeResult | null = null;
      let authorizedError = false;
      try {
        authorizedResult = await testInvokeApiGatewayRestMethod(
          restApiId,
          method.resourceId,
          method.httpMethod,
          {
            pathWithQueryString: method.resourcePath,
            headers: { Authorization: authorizationToken },
            queryStringParameters: {},
            body: null,
            stageVariables: {},
          }
        );
      } catch {
        authorizedError = true;
      }

      setVerificationResult({
        unauthorized: unauthorizedResult,
        authorized: authorizedResult,
        unauthorizedError,
        authorizedError,
      });
    } finally {
      setVerifying(false);
    }
  };

  return (
    <div data-testid="apigateway-protected-route-verification-section" style={sectionStyle}>
      <Heading
        as="h3"
        data-testid="apigateway-protected-route-verification-heading"
        style={sectionHeadingStyle}
      >
        Protected route verification
      </Heading>

      {state.kind === 'loading' && (
        <p data-testid="apigateway-protected-route-verification-loading" style={messageStyle}>
          Discovering protected routes&hellip;
        </p>
      )}

      {state.kind === 'error' && (
        <p data-testid="apigateway-protected-route-verification-load-error" style={messageStyle}>
          Unable to discover protected routes.
        </p>
      )}

      {state.kind === 'ready' && state.protectedMethods.length === 0 && (
        <p data-testid="apigateway-protected-route-verification-empty" style={messageStyle}>
          No protected routes found. Configure authorization on a method to test it.
        </p>
      )}

      {state.kind === 'ready' && state.protectedMethods.length > 0 && (
        <div style={formStyle}>
          <label
            style={labelStyle}
            htmlFor="apigateway-protected-route-verification-method"
          >
            Protected method
          </label>
          <select
            id="apigateway-protected-route-verification-method"
            data-testid="apigateway-protected-route-verification-method"
            style={inputStyle}
            value={selectedIndex}
            onChange={(event) => setSelectedIndex(Number(event.target.value))}
          >
            {state.protectedMethods.map((method, index) => (
              <option key={`${method.resourceId}|${method.httpMethod}`} value={index}>
                {method.httpMethod} {method.resourcePath} ({method.authorizationType})
              </option>
            ))}
          </select>

          <label style={labelStyle} htmlFor="apigateway-protected-route-verification-token">
            Authorization header (for authorized test)
          </label>
          <input
            id="apigateway-protected-route-verification-token"
            data-testid="apigateway-protected-route-verification-token"
            style={inputStyle}
            type="text"
            value={authorizationToken}
            onChange={(event) => setAuthorizationToken(event.target.value)}
            placeholder="Bearer token-value"
          />

          <button
            type="button"
            data-testid="apigateway-protected-route-verification-submit"
            style={buttonStyle}
            disabled={verifying}
            onClick={handleVerify}
          >
            {verifying ? 'Verifying…' : 'Verify authorization'}
          </button>
        </div>
      )}

      {verificationResult && (
        <div data-testid="apigateway-protected-route-verification-results" style={resultsContainerStyle}>
          <div style={verificationResult.unauthorizedError || verificationResult.unauthorized!.statusCode >= 400 ? passStyle : failStyle}>
            <div style={{ fontSize: 12, fontWeight: 600, marginBottom: 8 }}>
              ✓ Without authorization
            </div>
            {verificationResult.unauthorizedError ? (
              <div style={{ fontSize: 12, color: '#3fb950' }}>Request error (expected)</div>
            ) : (
              verificationResult.unauthorized && (
                <>
                  <div style={{ marginBottom: 6 }}>
                    <span style={statusBadgeStyle(verificationResult.unauthorized.statusCode)}>
                      {verificationResult.unauthorized.statusCode}
                    </span>
                  </div>
                  <div style={{ fontSize: 12 }}>
                    {verificationResult.unauthorized.statusCode >= 400
                      ? '✓ Auth enforced (request rejected)'
                      : '✗ Auth not enforced (request accepted)'}
                  </div>
                </>
              )
            )}
          </div>

          <div style={verificationResult.authorizedError ? failStyle : passStyle}>
            <div style={{ fontSize: 12, fontWeight: 600, marginBottom: 8 }}>
              With authorization header
            </div>
            {verificationResult.authorizedError ? (
              <div style={{ fontSize: 12, color: '#f85149' }}>Request error (unexpected)</div>
            ) : (
              verificationResult.authorized && (
                <>
                  <div style={{ marginBottom: 6 }}>
                    <span style={statusBadgeStyle(verificationResult.authorized.statusCode)}>
                      {verificationResult.authorized.statusCode}
                    </span>
                  </div>
                  <div style={{ fontSize: 12 }}>
                    {verificationResult.authorized.statusCode < 400
                      ? '✓ Request forwarded (auth accepted)'
                      : '✗ Auth rejected (likely invalid token format)'}
                  </div>
                </>
              )
            )}
          </div>
        </div>
      )}
    </div>
  );
}

export default ApiGatewayProtectedRouteVerificationSection;
