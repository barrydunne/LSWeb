import type { ComponentType } from 'react';

export interface ServiceListViewProps {
  serviceKey: string;
}

export interface ServiceDetailViewProps {
  serviceKey: string;
  resourceId: string;
}

export interface ServiceViewEntry {
  list?: ComponentType<ServiceListViewProps>;
  detail?: ComponentType<ServiceDetailViewProps>;
}

const registry = new Map<string, ServiceViewEntry>();

export function registerServiceView(serviceKey: string, entry: ServiceViewEntry): void {
  const existing = registry.get(serviceKey);
  registry.set(serviceKey, { ...existing, ...entry });
}

export function getServiceView(serviceKey: string): ServiceViewEntry | undefined {
  return registry.get(serviceKey);
}

export function clearServiceViews(): void {
  registry.clear();
}
