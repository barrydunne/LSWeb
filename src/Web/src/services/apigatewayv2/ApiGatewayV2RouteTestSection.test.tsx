import { afterEach, describe, expect, it, vi } from 'vitest';
import { cleanup, render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ThemeProvider } from '@primer/react';
import { ApiGatewayV2RouteTestSection } from './ApiGatewayV2RouteTestSection';
import * as client from '../../api/client';

function renderSection(apiId = 'api-1') {
  return render(
    <ThemeProvider colorMode="night">
      <ApiGatewayV2RouteTestSection apiId={apiId} />
    </ThemeProvider>,
  );
}

describe('ApiGatewayV2RouteTestSection', () => {
  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('does not invoke the client on mount', () => {
    const spy = vi.spyOn(client, 'testHttpApiRoute');

    renderSection();

    expect(spy).not.toHaveBeenCalled();
  });

  it('shows an unauthorized banner when the request is rejected without a token', async () => {
    const spy = vi.spyOn(client, 'testHttpApiRoute').mockResolvedValue({
      statusCode: 401,
      authorized: false,
      latencyMilliseconds: 5,
      headers: {},
      body: 'Unauthorized',
    });
    const user = userEvent.setup();
    renderSection('api-1');

    await user.clear(screen.getByTestId('apigatewayv2-route-test-path'));
    await user.type(screen.getByTestId('apigatewayv2-route-test-path'), '/orders');
    await user.click(screen.getByTestId('apigatewayv2-route-test-send'));

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-route-test-unauthorized')).toBeInTheDocument(),
    );
    const result = screen.getByTestId('apigatewayv2-route-test-result');
    expect(within(result).getByTestId('apigatewayv2-route-test-status')).toHaveTextContent(
      'Status 401',
    );
    expect(spy).toHaveBeenCalledWith('api-1', {
      stage: '$default',
      method: 'GET',
      path: '/orders',
      token: null,
      body: null,
    });
  });

  it('shows an authorized banner and body when a valid token is supplied', async () => {
    const spy = vi.spyOn(client, 'testHttpApiRoute').mockResolvedValue({
      statusCode: 200,
      authorized: true,
      latencyMilliseconds: 11,
      headers: { 'Content-Type': 'application/json' },
      body: '{"ok":true}',
    });
    const user = userEvent.setup();
    renderSection('api-1');

    await user.clear(screen.getByTestId('apigatewayv2-route-test-stage'));
    await user.type(screen.getByTestId('apigatewayv2-route-test-stage'), 'prod');
    await user.clear(screen.getByTestId('apigatewayv2-route-test-path'));
    await user.type(screen.getByTestId('apigatewayv2-route-test-path'), '/orders');
    await user.type(screen.getByTestId('apigatewayv2-route-test-token'), 'token-123');
    await user.type(screen.getByTestId('apigatewayv2-route-test-body'), '{{"a":1}');
    await user.selectOptions(screen.getByTestId('apigatewayv2-route-test-method'), 'POST');
    await user.click(screen.getByTestId('apigatewayv2-route-test-send'));

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-route-test-authorized')).toBeInTheDocument(),
    );
    const result = screen.getByTestId('apigatewayv2-route-test-result');
    expect(within(result).getByTestId('apigatewayv2-route-test-latency')).toHaveTextContent(
      '11 ms',
    );
    expect(within(result).getByTestId('apigatewayv2-route-test-body-result')).toHaveTextContent(
      '{"ok":true}',
    );
    expect(spy).toHaveBeenCalledWith('api-1', {
      stage: 'prod',
      method: 'POST',
      path: '/orders',
      token: 'token-123',
      body: '{"a":1}',
    });
  });

  it('shows an error message when the invocation fails', async () => {
    vi.spyOn(client, 'testHttpApiRoute').mockRejectedValue(new Error('boom'));
    const user = userEvent.setup();
    renderSection();

    await user.click(screen.getByTestId('apigatewayv2-route-test-send'));

    await waitFor(() =>
      expect(screen.getByTestId('apigatewayv2-route-test-error')).toBeInTheDocument(),
    );
  });
});
