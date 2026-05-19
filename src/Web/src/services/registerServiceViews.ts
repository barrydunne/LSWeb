import { lazy } from 'react';
import { registerServiceView } from './serviceViewRegistry';

const LambdaListView = lazy(() => import('./lambda/LambdaListView'));
const LambdaDetailView = lazy(() => import('./lambda/LambdaDetailView'));

let registered = false;

/**
 * Register every built-in service view exactly once. Imported for its side effect at app start.
 */
export function registerServiceViews(): void {
  if (registered) {
    return;
  }
  registered = true;

  registerServiceView('lambda', { list: LambdaListView, detail: LambdaDetailView });
}

registerServiceViews();
