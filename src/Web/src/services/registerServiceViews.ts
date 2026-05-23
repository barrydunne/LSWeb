import { lazy } from 'react';
import { registerServiceView } from './serviceViewRegistry';

const LambdaListView = lazy(() => import('./lambda/LambdaListView'));
const LambdaDetailView = lazy(() => import('./lambda/LambdaDetailView'));
const S3ListView = lazy(() => import('./s3/S3ListView'));
const S3DetailView = lazy(() => import('./s3/S3DetailView'));
const SqsListView = lazy(() => import('./sqs/SqsListView'));
const SqsDetailView = lazy(() => import('./sqs/SqsDetailView'));

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
  registerServiceView('s3', { list: S3ListView, detail: S3DetailView });
  registerServiceView('sqs', { list: SqsListView, detail: SqsDetailView });
}

registerServiceViews();
