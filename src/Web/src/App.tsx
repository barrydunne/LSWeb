import { useEffect, useState } from 'react';
import { Heading, PageLayout, Text, ThemeProvider } from '@primer/react';
import { BrowserRouter, Outlet, Route, Routes } from 'react-router-dom';
import { getLiveness } from './api/client';
import { ActivityLogPanel } from './components/ActivityLogPanel';
import { Breadcrumbs } from './components/Breadcrumbs';
import { ConnectivityIndicator } from './components/ConnectivityIndicator';
import { DashboardPage } from './components/DashboardPage';
import { GlobalSearchBar } from './components/GlobalSearchBar';
import { KeyboardShortcuts } from './components/KeyboardShortcuts';
import { NotificationCenter } from './components/NotificationCenter';
import { DiagnosticsPage } from './pages/DiagnosticsPage';
import { HomePage } from './pages/HomePage';
import { NotFoundPage } from './pages/NotFoundPage';
import { ResourceDetailPage } from './pages/ResourceDetailPage';
import { ServicePage } from './pages/ServicePage';

/**
 * Persistent application chrome. The header, breadcrumbs, notifications,
 * keyboard shortcuts and activity log stay mounted across every route while
 * the routed page renders into the {@link Outlet}.
 */
function AppShell() {
  const [status, setStatus] = useState('checking\u2026');

  useEffect(() => {
    const controller = new AbortController();
    getLiveness(controller.signal)
      .then((result) => setStatus(result.status))
      .catch(() => setStatus('unavailable'));
    return () => controller.abort();
  }, []);

  return (
    <div data-testid="app-root" style={{ minHeight: '100vh' }}>
      <NotificationCenter />
      <KeyboardShortcuts />
      <PageLayout>
        <PageLayout.Header>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 16 }}>
            <Heading as="h1" data-testid="app-title">
              LocalStack Web
            </Heading>
            <GlobalSearchBar />
            <ConnectivityIndicator />
          </div>
        </PageLayout.Header>
        <PageLayout.Content>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
            <Breadcrumbs />
            <Text data-testid="health-status">Service status: {status}</Text>
            <Outlet />
            <ActivityLogPanel />
          </div>
        </PageLayout.Content>
      </PageLayout>
    </div>
  );
}

export function App() {
  return (
    <ThemeProvider colorMode="night">
      <BrowserRouter>
        <Routes>
          <Route element={<AppShell />}>
            <Route index element={<HomePage />} />
            <Route path="dashboard" element={<DashboardPage />} />
            <Route path="diagnostics" element={<DiagnosticsPage />} />
            <Route path="services/:serviceKey" element={<ServicePage />} />
            <Route path="services/:serviceKey/*" element={<ResourceDetailPage />} />
            <Route path="*" element={<NotFoundPage />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </ThemeProvider>
  );
}
