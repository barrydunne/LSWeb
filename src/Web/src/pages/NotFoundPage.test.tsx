import { describe, expect, it } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ThemeProvider } from '@primer/react';
import { MemoryRouter } from 'react-router-dom';
import { NotFoundPage } from './NotFoundPage';

function renderPage() {
  return render(
    <MemoryRouter>
      <ThemeProvider colorMode="night">
        <NotFoundPage />
      </ThemeProvider>
    </MemoryRouter>,
  );
}

describe('NotFoundPage', () => {
  it('renders a not-found message with a home link', () => {
    renderPage();

    expect(screen.getByTestId('not-found-heading')).toHaveTextContent('Page not found');
    expect(screen.getByTestId('not-found-home-link')).toHaveAttribute('href', '/');
  });
});
