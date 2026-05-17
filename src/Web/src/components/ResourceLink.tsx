import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Link } from 'react-router-dom';
import { resolveReference, type ResolvedReferenceResult } from '../api/client';

const linkStyle: CSSProperties = {
  color: '#58a6ff',
  textDecoration: 'none',
};

const textStyle: CSSProperties = {
  color: 'inherit',
};

export function ResourceLink({
  reference,
  service,
  label,
}: {
  reference: string;
  service?: string;
  label?: string;
}) {
  const [resolved, setResolved] = useState<ResolvedReferenceResult | null>(null);

  useEffect(() => {
    const controller = new AbortController();
    resolveReference(reference, service, controller.signal)
      .then(setResolved)
      .catch(() => setResolved(null));
    return () => controller.abort();
  }, [reference, service]);

  const text = label ?? resolved?.resourceId ?? reference;

  if (resolved) {
    return (
      <Link to={resolved.route} data-testid="resource-link" style={linkStyle}>
        {text}
      </Link>
    );
  }

  return (
    <span data-testid="resource-link" style={textStyle}>
      {text}
    </span>
  );
}
