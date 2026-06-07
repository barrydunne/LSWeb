import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading, Text } from '@primer/react';
import {
  applySeedTemplate,
  getSeedTemplates,
  type SeedOutcomeResult,
  type SeedTemplateItem,
} from '../api/client';

type PanelState =
  | { kind: 'loading' }
  | { kind: 'ready'; templates: SeedTemplateItem[] }
  | { kind: 'error' };

const sectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
};

const gridStyle: CSSProperties = {
  display: 'grid',
  gridTemplateColumns: 'repeat(auto-fill, minmax(260px, 1fr))',
  gap: 16,
};

const cardStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const resourceListStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 4,
  listStyle: 'none',
  margin: 0,
  padding: 0,
  fontSize: 13,
  opacity: 0.85,
};

const buttonStyle: CSSProperties = {
  alignSelf: 'flex-start',
  padding: '6px 14px',
  borderRadius: 6,
  border: '1px solid #2ea043',
  background: '#238636',
  color: '#ffffff',
  fontSize: 13,
  cursor: 'pointer',
};

const buttonDisabledStyle: CSSProperties = {
  ...buttonStyle,
  opacity: 0.6,
  cursor: 'not-allowed',
};

const messageStyle: CSSProperties = {
  fontSize: 13,
};

function outcomeMessage(outcome: SeedOutcomeResult): string {
  return `${outcome.succeededCount} of ${outcome.totalCount} resource(s) created; ${outcome.failedCount} failed.`;
}

export function SeedTemplatesPanel() {
  const [state, setState] = useState<PanelState>({ kind: 'loading' });
  const [pendingId, setPendingId] = useState<string | null>(null);
  const [outcomes, setOutcomes] = useState<Map<string, SeedOutcomeResult>>(new Map());
  const [failures, setFailures] = useState<Set<string>>(new Set());

  useEffect(() => {
    const controller = new AbortController();
    getSeedTemplates(controller.signal)
      .then((result) => setState({ kind: 'ready', templates: result.templates }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, []);

  const handleApply = (template: SeedTemplateItem) => {
    if (!window.confirm(`Create the ${template.resources.length} resource(s) in "${template.name}"?`)) {
      return;
    }
    setPendingId(template.id);
    setFailures((current) => {
      const next = new Set(current);
      next.delete(template.id);
      return next;
    });
    applySeedTemplate(template.id)
      .then((outcome) => {
        setOutcomes((current) => new Map(current).set(template.id, outcome));
      })
      .catch(() => {
        setFailures((current) => new Set(current).add(template.id));
      })
      .finally(() => setPendingId((current) => (current === template.id ? null : current)));
  };

  if (state.kind === 'loading') {
    return (
      <section data-testid="seed-panel" style={sectionStyle}>
        <Heading as="h3" data-testid="seed-heading" style={{ fontSize: 16 }}>
          Quick start templates
        </Heading>
        <Text data-testid="seed-loading" style={messageStyle}>
          Loading templates&hellip;
        </Text>
      </section>
    );
  }

  if (state.kind === 'error') {
    return (
      <section data-testid="seed-panel" style={sectionStyle}>
        <Heading as="h3" data-testid="seed-heading" style={{ fontSize: 16 }}>
          Quick start templates
        </Heading>
        <Text data-testid="seed-error" style={messageStyle}>
          Unable to load templates.
        </Text>
      </section>
    );
  }

  return (
    <section data-testid="seed-panel" style={sectionStyle}>
      <Heading as="h3" data-testid="seed-heading" style={{ fontSize: 16 }}>
        Quick start templates
      </Heading>
      <Text data-testid="seed-subtitle" style={{ fontSize: 13, opacity: 0.8 }}>
        Create a ready-made set of resources to start experimenting.
      </Text>

      {state.templates.length === 0 ? (
        <Text data-testid="seed-empty" style={messageStyle}>
          No templates are available.
        </Text>
      ) : (
        <div data-testid="seed-grid" style={gridStyle}>
          {state.templates.map((template) => {
            const outcome = outcomes.get(template.id);
            const failed = failures.has(template.id);
            const pending = pendingId === template.id;
            return (
              <div key={template.id} data-testid="seed-card" style={cardStyle}>
                <Heading as="h4" data-testid="seed-card-name" style={{ fontSize: 15 }}>
                  {template.name}
                </Heading>
                <Text data-testid="seed-card-description" style={{ fontSize: 13, opacity: 0.85 }}>
                  {template.description}
                </Text>
                <ul data-testid="seed-card-resources" style={resourceListStyle}>
                  {template.resources.map((resource) => (
                    <li key={`${resource.serviceKey}:${resource.name}`} data-testid="seed-card-resource">
                      {resource.resourceType}: {resource.name}
                    </li>
                  ))}
                </ul>
                <button
                  type="button"
                  data-testid="seed-apply-button"
                  onClick={() => handleApply(template)}
                  disabled={pending}
                  style={pending ? buttonDisabledStyle : buttonStyle}
                >
                  {pending ? 'Creating\u2026' : 'Create resources'}
                </button>
                {outcome ? (
                  <Text data-testid="seed-card-outcome" style={messageStyle}>
                    {outcomeMessage(outcome)}
                  </Text>
                ) : null}
                {failed ? (
                  <Text data-testid="seed-card-failure" style={messageStyle}>
                    The template could not be applied.
                  </Text>
                ) : null}
              </div>
            );
          })}
        </div>
      )}
    </section>
  );
}
