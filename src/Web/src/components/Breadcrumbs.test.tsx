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
    expect(items).toHaveLength(4);
    expect(items[0]).toHaveAttribute('href', '/');
    expect(items[1]).toHaveAttribute('href', '/services');
    expect(items[2]).toHaveAttribute('href', '/services/sqs');
    expect(items[3]).toHaveTextContent('orders');
    expect(items[3]).toHaveAttribute('aria-current', 'page');
    expect(items[3]).not.toHaveAttribute('href');
  });

  it('decodes percent-encoded path segments', () => {
    renderBreadcrumbs('/services/sqs/my%20queue');

    const items = screen.getAllByTestId('breadcrumb-item');
    expect(items[3]).toHaveTextContent('my queue');
  });

  it('falls back to the current location when no pathname is supplied', () => {
    renderBreadcrumbs(undefined, '/services');

    const items = screen.getAllByTestId('breadcrumb-item');
    expect(items).toHaveLength(2);
    expect(items[1]).toHaveTextContent('services');
  });
});
