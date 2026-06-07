import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { cleanup, render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ThemeProvider } from '@primer/react';
import { SeedTemplatesPanel } from './SeedTemplatesPanel';
import {
  applySeedTemplate,
  getSeedTemplates,
  type SeedOutcomeResult,
  type SeedTemplateItem,
} from '../api/client';

vi.mock('../api/client');

const getSeedTemplatesMock = vi.mocked(getSeedTemplates);
const applySeedTemplateMock = vi.mocked(applySeedTemplate);

function template(overrides: Partial<SeedTemplateItem> & { id: string }): SeedTemplateItem {
  return {
    id: overrides.id,
    name: overrides.name ?? overrides.id,
    description: overrides.description ?? 'A starter template.',
    resources: overrides.resources ?? [
      { serviceKey: 'sqs', resourceType: 'Queue', name: 'seed-orders-queue' },
    ],
  };
}

function outcome(overrides: Partial<SeedOutcomeResult> & { templateId: string }): SeedOutcomeResult {
  return {
    operationId: overrides.operationId ?? 'op-1',
    templateId: overrides.templateId,
    totalCount: overrides.totalCount ?? 2,
    succeededCount: overrides.succeededCount ?? 2,
    failedCount: overrides.failedCount ?? 0,
    overallState: overrides.overallState ?? 'Succeeded',
    items: overrides.items ?? [],
  };
}

function renderPanel() {
  return render(
    <ThemeProvider colorMode="night">
      <SeedTemplatesPanel />
    </ThemeProvider>,
  );
}

describe('SeedTemplatesPanel', () => {
  beforeEach(() => {
    getSeedTemplatesMock.mockResolvedValue({ templates: [] });
    applySeedTemplateMock.mockResolvedValue(outcome({ templateId: 'messaging-starter' }));
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows a loading message while templates are pending', () => {
    getSeedTemplatesMock.mockReturnValue(new Promise<never>(() => {}));

    renderPanel();

    expect(screen.getByTestId('seed-loading')).toBeInTheDocument();
  });

  it('shows an error message when templates fail to load', async () => {
    getSeedTemplatesMock.mockRejectedValue(new Error('boom'));

    renderPanel();

    expect(await screen.findByTestId('seed-error')).toBeInTheDocument();
  });

  it('shows an empty message when there are no templates', async () => {
    getSeedTemplatesMock.mockResolvedValue({ templates: [] });

    renderPanel();

    expect(await screen.findByTestId('seed-empty')).toBeInTheDocument();
  });

  it('renders a card for every template with its resources', async () => {
    getSeedTemplatesMock.mockResolvedValue({
      templates: [
        template({
          id: 'messaging-starter',
          name: 'Messaging starter',
          description: 'Queue and topic.',
          resources: [
            { serviceKey: 'sqs', resourceType: 'Queue', name: 'seed-orders-queue' },
            { serviceKey: 'sns', resourceType: 'Topic', name: 'seed-orders-topic' },
          ],
        }),
      ],
    });

    renderPanel();

    const card = await screen.findByTestId('seed-card');
    expect(within(card).getByTestId('seed-card-name')).toHaveTextContent('Messaging starter');
    expect(within(card).getByTestId('seed-card-description')).toHaveTextContent('Queue and topic.');
    const resources = within(card).getAllByTestId('seed-card-resource');
    expect(resources).toHaveLength(2);
    expect(resources[0]).toHaveTextContent('Queue: seed-orders-queue');
    expect(resources[1]).toHaveTextContent('Topic: seed-orders-topic');
  });

  it('does not apply the template when the confirmation is declined', async () => {
    const user = userEvent.setup();
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(false);
    getSeedTemplatesMock.mockResolvedValue({ templates: [template({ id: 'messaging-starter' })] });

    renderPanel();

    await user.click(await screen.findByTestId('seed-apply-button'));

    expect(applySeedTemplateMock).not.toHaveBeenCalled();
    confirmSpy.mockRestore();
  });

  it('applies the template and shows the outcome when confirmed', async () => {
    const user = userEvent.setup();
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);
    getSeedTemplatesMock.mockResolvedValue({ templates: [template({ id: 'messaging-starter' })] });
    applySeedTemplateMock.mockResolvedValue(
      outcome({ templateId: 'messaging-starter', totalCount: 2, succeededCount: 2, failedCount: 0 }),
    );

    renderPanel();

    await user.click(await screen.findByTestId('seed-apply-button'));

    expect(applySeedTemplateMock).toHaveBeenCalledWith('messaging-starter');
    expect(await screen.findByTestId('seed-card-outcome')).toHaveTextContent(
      '2 of 2 resource(s) created; 0 failed.',
    );
    confirmSpy.mockRestore();
  });

  it('disables the button and shows progress text while applying', async () => {
    const user = userEvent.setup();
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);
    getSeedTemplatesMock.mockResolvedValue({ templates: [template({ id: 'messaging-starter' })] });
    applySeedTemplateMock.mockReturnValue(new Promise<never>(() => {}));

    renderPanel();

    const button = await screen.findByTestId('seed-apply-button');
    await user.click(button);

    await waitFor(() => expect(screen.getByTestId('seed-apply-button')).toBeDisabled());
    expect(screen.getByTestId('seed-apply-button')).toHaveTextContent('Creating');
    confirmSpy.mockRestore();
  });

  it('shows a failure message when applying the template throws', async () => {
    const user = userEvent.setup();
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);
    getSeedTemplatesMock.mockResolvedValue({ templates: [template({ id: 'messaging-starter' })] });
    applySeedTemplateMock.mockRejectedValue(new Error('boom'));

    renderPanel();

    await user.click(await screen.findByTestId('seed-apply-button'));

    expect(await screen.findByTestId('seed-card-failure')).toBeInTheDocument();
    confirmSpy.mockRestore();
  });

  it('clears a previous failure when the template is retried', async () => {
    const user = userEvent.setup();
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);
    getSeedTemplatesMock.mockResolvedValue({ templates: [template({ id: 'messaging-starter' })] });
    applySeedTemplateMock.mockRejectedValueOnce(new Error('boom'));
    applySeedTemplateMock.mockResolvedValueOnce(outcome({ templateId: 'messaging-starter' }));

    renderPanel();

    const button = await screen.findByTestId('seed-apply-button');
    await user.click(button);
    expect(await screen.findByTestId('seed-card-failure')).toBeInTheDocument();

    await user.click(button);
    await waitFor(() => expect(screen.queryByTestId('seed-card-failure')).not.toBeInTheDocument());
    expect(await screen.findByTestId('seed-card-outcome')).toBeInTheDocument();
    confirmSpy.mockRestore();
  });

  it('keeps a second template pending when an earlier apply settles', async () => {
    const user = userEvent.setup();
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);
    getSeedTemplatesMock.mockResolvedValue({
      templates: [
        template({ id: 'messaging-starter', name: 'Messaging' }),
        template({ id: 'storage-starter', name: 'Storage' }),
      ],
    });

    let resolveFirst!: (value: SeedOutcomeResult) => void;
    applySeedTemplateMock.mockReturnValueOnce(
      new Promise<SeedOutcomeResult>((resolve) => {
        resolveFirst = resolve;
      }),
    );
    applySeedTemplateMock.mockReturnValueOnce(new Promise<never>(() => {}));

    renderPanel();

    const buttons = await screen.findAllByTestId('seed-apply-button');
    await user.click(buttons[0]);
    await waitFor(() => expect(buttons[0]).toBeDisabled());
    await user.click(buttons[1]);
    await waitFor(() => expect(buttons[1]).toBeDisabled());

    resolveFirst(outcome({ templateId: 'messaging-starter' }));

    await waitFor(() => expect(buttons[0]).not.toBeDisabled());
    expect(buttons[1]).toBeDisabled();
    confirmSpy.mockRestore();
  });
});
