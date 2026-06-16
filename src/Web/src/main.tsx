import { createRoot } from 'react-dom/client';
import { App } from './App';
import './services/registerServiceViews';
import './index.css';

const mutatingMethods = new Set(['POST', 'PUT', 'PATCH', 'DELETE']);
const inFlightMutations = new Map<string, Promise<Response>>();
const originalFetch = window.fetch.bind(window);

window.fetch = (input: RequestInfo | URL, init?: RequestInit): Promise<Response> => {
	const method = (init?.method ?? (input instanceof Request ? input.method : 'GET')).toUpperCase();
	if (!mutatingMethods.has(method)) {
		return originalFetch(input, init);
	}

	const requestUrl = typeof input === 'string'
		? input
		: input instanceof URL
			? input.toString()
			: input.url;

	// SignalR negotiate requests must never be deduplicated: each hub connection
	// instance needs its own unique connection ID from the server. Deduplicating
	// two concurrent negotiate calls would return the same ID to both instances,
	// which causes the second WS/transport to get HTTP 409 (connection already
	// registered under a different transport).
	if (requestUrl.includes('/hub/')) {
		return originalFetch(input, init);
	}

	const requestBody = typeof init?.body === 'string' ? init.body : '';
	const dedupeKey = `${method} ${requestUrl} ${requestBody}`;

	const existingRequest = inFlightMutations.get(dedupeKey);
	if (existingRequest !== undefined) {
		return existingRequest.then((response) => response.clone());
	}

	const requestPromise = originalFetch(input, init);
	inFlightMutations.set(dedupeKey, requestPromise);

	return requestPromise
		.then((response) => response.clone())
		.finally(() => {
			inFlightMutations.delete(dedupeKey);
		});
};

// React's development-only double-invocation of mount effects starts, immediately
// aborts, and restarts every on-mount fetch and the SignalR connection, which the
// browser logs as a stream of net::ERR_ABORTED entries. That console noise distracts
// developers debugging their own apps against the console, so the app is mounted
// without the StrictMode wrapper.
createRoot(document.getElementById('root')!).render(<App />);
