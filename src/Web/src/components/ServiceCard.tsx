import type { CSSProperties } from 'react';
import { Heading, Label, Text } from '@primer/react';
import type { CatalogueServiceItem } from '../api/client';

type LabelVariant = 'success' | 'danger' | 'secondary';

function availabilityVariant(availability: string): LabelVariant {
  if (availability === 'Available') {
    return 'success';
  }
  if (availability === 'Unavailable') {
    return 'danger';
  }
  return 'secondary';
}

export function ServiceCard({
  service,
  availability = 'Available',
}: {
  service: CatalogueServiceItem;
  availability?: string;
}) {
  const isUnsupported = !service.supported;
  const isUnavailable = availability === 'Unavailable';
  const dimmed = isUnsupported || isUnavailable;

  const cardStyle: CSSProperties = {
    display: 'flex',
    flexDirection: 'column',
    gap: 8,
    padding: 16,
    borderRadius: 6,
    border: '1px solid #30363d',
    background: '#161b22',
    textDecoration: 'none',
    color: 'inherit',
    opacity: dimmed ? 0.55 : 1,
  };

  const body = (
    <>
      <Text data-testid="service-card-icon" aria-hidden style={{ fontSize: 12, opacity: 0.7 }}>
        {service.iconHint}
      </Text>
      <Heading as="h3" data-testid="service-card-name" style={{ fontSize: 16 }}>
        {service.displayName}
      </Heading>
      <span style={{ display: 'inline-flex', gap: 8, flexWrap: 'wrap' }}>
        <Label data-testid="service-card-category">{service.category}</Label>
        <Label variant={availabilityVariant(availability)} data-testid="service-card-availability">
          {availability}
        </Label>
        {isUnsupported ? (
          <Label variant="danger" data-testid="service-card-unsupported">
            Unsupported
          </Label>
        ) : null}
      </span>
      {isUnsupported ? (
        <Text data-testid="service-card-support-detail" style={{ fontSize: 12, opacity: 0.8 }}>
          {service.supportDetail ?? 'This service is not supported by the current backend.'}
        </Text>
      ) : null}
    </>
  );

  if (isUnsupported) {
    return (
      <div data-testid={`service-card-${service.key}`} role="group" aria-disabled="true" style={cardStyle}>
        {body}
      </div>
    );
  }

  return (
    <a
      href={service.route}
      data-testid={`service-card-${service.key}`}
      aria-disabled={isUnavailable}
      style={cardStyle}
    >
      {body}
    </a>
  );
}
