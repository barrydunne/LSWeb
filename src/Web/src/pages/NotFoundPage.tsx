import type { CSSProperties } from 'react';
import { Heading } from '@primer/react';
import { Link } from 'react-router-dom';
import { EmptyState } from '../components/EmptyState';

const sectionStyle: CSSProperties = { display: 'flex', flexDirection: 'column', gap: 16 };
const linkStyle: CSSProperties = { color: '#58a6ff', textDecoration: 'none' };

export function NotFoundPage() {
  return (
    <section data-testid="not-found-page" style={sectionStyle}>
      <Heading as="h2" data-testid="not-found-heading" style={{ fontSize: 20 }}>
        Page not found
      </Heading>
      <EmptyState
        variant="no-matches"
        message="That page doesn't exist."
        action={
          <Link to="/" data-testid="not-found-home-link" style={linkStyle}>
            Back to home
          </Link>
        }
      />
    </section>
  );
}
