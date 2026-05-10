import { useEffect, useState } from 'react';
import { Heading, PageLayout, Text, ThemeProvider } from '@primer/react';
import { getLiveness } from './api/client';
import { ConnectivityIndicator } from './components/ConnectivityIndicator';

export function App() {
  const [status, setStatus] = useState('checking\u2026');

  useEffect(() => {
    const controller = new AbortController();
    getLiveness(controller.signal)
      .then((result) => setStatus(result.status))
      .catch(() => setStatus('unavailable'));
    return () => controller.abort();
  }, []);

  return (
    <ThemeProvider colorMode="night">
      <div data-testid="app-root" style={{ minHeight: '100vh' }}>
        <PageLayout>
          <PageLayout.Header>
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 16 }}>
              <Heading as="h1" data-testid="app-title">
                LocalStack Web
              </Heading>
              <ConnectivityIndicator />
            </div>
          </PageLayout.Header>
          <PageLayout.Content>
            <Text data-testid="health-status">Service status: {status}</Text>
          </PageLayout.Content>
        </PageLayout>
      </div>
    </ThemeProvider>
  );
}
