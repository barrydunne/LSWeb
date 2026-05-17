import { useEffect, useState } from 'react';
import { getCatalogue, type CatalogueServiceItem } from '../api/client';

export type CatalogueServiceState =
  | { kind: 'loading' }
  | { kind: 'unknown' }
  | { kind: 'error' }
  | { kind: 'ready'; service: CatalogueServiceItem };

/**
 * Loads the service catalogue and resolves a single service by its key.
 * Shared by the service-page and resource-detail shells so that an
 * unrecognised serviceKey degrades to a friendly not-found state.
 */
export function useCatalogueService(serviceKey: string): CatalogueServiceState {
  const [state, setState] = useState<CatalogueServiceState>({ kind: 'loading' });

  useEffect(() => {
    const controller = new AbortController();
    setState({ kind: 'loading' });
    getCatalogue(controller.signal)
      .then((catalogue) => {
        const service = catalogue.services.find((candidate) => candidate.key === serviceKey);
        setState(service ? { kind: 'ready', service } : { kind: 'unknown' });
      })
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [serviceKey]);

  return state;
}
