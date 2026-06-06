import { afterEach, describe, expect, it } from 'vitest';
import { registerServiceViews } from './registerServiceViews';
import { clearServiceViews, getServiceView } from './serviceViewRegistry';

describe('registerServiceViews', () => {
  afterEach(() => {
    clearServiceViews();
  });

  it('registers the lambda and s3 views', () => {
    // The module self-invokes on import, so the views are already registered.
    const lambda = getServiceView('lambda');
    expect(lambda?.list).toBeDefined();
    expect(lambda?.detail).toBeDefined();

    const s3 = getServiceView('s3');
    expect(s3?.list).toBeDefined();
    expect(s3?.detail).toBeDefined();

    const sqs = getServiceView('sqs');
    expect(sqs?.list).toBeDefined();
    expect(sqs?.detail).toBeDefined();

    const logs = getServiceView('cloudwatch-logs');
    expect(logs?.list).toBeDefined();
    expect(logs?.detail).toBeDefined();

    const ssm = getServiceView('ssm-parameter-store');
    expect(ssm?.list).toBeDefined();

    const sns = getServiceView('sns');
    expect(sns?.list).toBeDefined();
    expect(sns?.detail).toBeDefined();

    const iam = getServiceView('iam');
    expect(iam?.list).toBeDefined();
    expect(iam?.detail).toBeDefined();

    const stepFunctions = getServiceView('step-functions');
    expect(stepFunctions?.list).toBeDefined();
    expect(stepFunctions?.detail).toBeDefined();

    const eventbridge = getServiceView('eventbridge');
    expect(eventbridge?.list).toBeDefined();

    const cognito = getServiceView('cognito');
    expect(cognito?.list).toBeDefined();
    expect(cognito?.detail).toBeDefined();
  });

  it('is idempotent and does not re-register after the first call', () => {
    clearServiceViews();

    // Guard short-circuits because registration already ran during import.
    registerServiceViews();

    expect(getServiceView('lambda')).toBeUndefined();
  });
});
