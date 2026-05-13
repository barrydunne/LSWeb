import type { CSSProperties } from 'react';

interface Crumb {
  label: string;
  href: string;
}

function buildCrumbs(pathname: string): Crumb[] {
  const segments = pathname.split('/').filter((segment) => segment.length > 0);
  const crumbs: Crumb[] = [{ label: 'Home', href: '/' }];
  let cumulative = '';
  for (const segment of segments) {
    cumulative += `/${segment}`;
    crumbs.push({ label: decodeURIComponent(segment), href: cumulative });
  }
  return crumbs;
}

const listStyle: CSSProperties = { display: 'flex', flexWrap: 'wrap', gap: 8, listStyle: 'none', margin: 0, padding: 0, fontSize: 13 };
const itemStyle: CSSProperties = { display: 'flex', alignItems: 'center', gap: 8 };
const linkStyle: CSSProperties = { color: '#58a6ff', textDecoration: 'none' };
const currentStyle: CSSProperties = { color: 'inherit', opacity: 0.8 };
const separatorStyle: CSSProperties = { opacity: 0.5 };

export function Breadcrumbs({ pathname = window.location.pathname }: { pathname?: string }) {
  const crumbs = buildCrumbs(pathname);
  return (
    <nav aria-label="Breadcrumb" data-testid="breadcrumbs">
      <ol style={listStyle}>
        {crumbs.map((crumb, index) => {
          const isLast = index === crumbs.length - 1;
          return (
            <li key={crumb.href} style={itemStyle}>
              {index > 0 ? (<span aria-hidden style={separatorStyle}>/</span>) : null}
              {isLast ? (
                <span data-testid="breadcrumb-item" aria-current="page" style={currentStyle}>{crumb.label}</span>
              ) : (
                <a data-testid="breadcrumb-item" href={crumb.href} style={linkStyle}>{crumb.label}</a>
              )}
            </li>
          );
        })}
      </ol>
    </nav>
  );
}
