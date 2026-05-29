import { afterEach, describe, expect, it } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { MemoryRouter } from 'react-router-dom';
import { Breadcrumbs } from './Breadcrumbs';

function renderBreadcrumbs(pathname?: string, locationPath = '/') {
  return render(
    <MemoryRouter initialEntries={[locationPath]}>
      <ThemeProvider colorMode="night">
        <Breadcrumbs pathname={pathname} />
      </ThemeProvider>
    </MemoryRouter>,
  );
}

describe('Breadcrumbs', () => {
  afterEach(() => {
    window.history.pushState({}, '', '/');
  });

  it('renders only the home crumb at the root path', () => {
    renderBreadcrumbs('/');

    const items = screen.getAllByTestId('breadcrumb-item');
    expect(items).toHaveLength(1);
    expect(items[0]).toHaveTextContent('Home');
    expect(items[0]).toHaveAttribute('aria-current', 'page');
  });

  it('builds a cumulative trail with the last segment marked current', () => {
    renderBreadcrumbs('/services/sqs/orders');

    const items = screen.getAllByTestId('breadcrumb-item');
    expect(items).toHaveLength(3);
    expect(items[0]).toHaveAttribute('href', '/');
    expect(items[1]).toHaveAttribute('href', '/services/sqs');
    expect(items[1]).toHaveTextContent('sqs');
    expect(items[2]).toHaveTextContent('orders');
    expect(items[2]).toHaveAttribute('aria-current', 'page');
    expect(items[2]).not.toHaveAttribute('href');
  });

  it('omits the non-navigable services prefix from the trail', () => {
    renderBreadcrumbs('/services/lambda/my-fn');

    const items = screen.getAllByTestId('breadcrumb-item');
    expect(items).toHaveLength(3);
    expect(screen.queryByText('services')).not.toBeInTheDocument();
    expect(items[0]).toHaveTextContent('Home');
    expect(items[1]).toHaveTextContent('lambda');
    expect(items[1]).toHaveAttribute('href', '/services/lambda');
    expect(items[2]).toHaveTextContent('my-fn');
    expect(items[2]).toHaveAttribute('aria-current', 'page');
  });

  it('keeps a leading non-service segment such as dashboard', () => {
    renderBreadcrumbs('/dashboard');

    const items = screen.getAllByTestId('breadcrumb-item');
    expect(items).toHaveLength(2);
    expect(items[1]).toHaveTextContent('dashboard');
    expect(items[1]).toHaveAttribute('aria-current', 'page');
  });

  it('decodes percent-encoded path segments', () => {
    renderBreadcrumbs('/services/sqs/my%20queue');

    const items = screen.getAllByTestId('breadcrumb-item');
    expect(items[2]).toHaveTextContent('my queue');
  });

  it('falls back to the current location when no pathname is supplied', () => {
    renderBreadcrumbs(undefined, '/services/sqs');

    const items = screen.getAllByTestId('breadcrumb-item');
    expect(items).toHaveLength(2);
    expect(items[1]).toHaveTextContent('sqs');
  });
});
