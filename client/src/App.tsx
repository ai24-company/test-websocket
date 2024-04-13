import './App.scss';

import { Fragment, useEffect } from 'react';
import { EventStreamContentType, fetchEventSource } from '@microsoft/fetch-event-source';

import { type IncomingMessage } from './types';
import { useActions, useAppSelector } from './hooks/redux-hooks.ts';
import { Messages } from './components/messages';
import { Footer } from './components/footer';

class RetriableError extends Error { }
class FatalError extends Error { }

export default function App() {
	const isLoading = useAppSelector(state => state.chat.isLoading);
	const { messagesReceived, toggleLoading } = useActions();

	useEffect(() => {
		const controller = new AbortController();
		toggleLoading(true);

		async function fetchInit() {
			await fetchEventSource(`${import.meta.env.VITE_API_URL}/send-text`, {
				method: 'POST',
				headers: {
					'Content-Type': 'application/json',
				},
				body: JSON.stringify({
					typeChat: 'init',
					incomeMessage: '',
				}),
				async onopen(response) {
					if (response.ok && response.headers.get('content-type') === EventStreamContentType) {
						toggleLoading(false);
						return; // everything's good
					} else if (response.status >= 400 && response.status < 500 && response.status !== 429) {
						// client-side errors are usually non-retriable:
						throw new FatalError();
					} else {
						throw new RetriableError();
					}
				},
				onmessage(event) {
					try {
						const parsedData = JSON.parse(event.data) as IncomingMessage;
						messagesReceived(parsedData);

						if (parsedData.type === 'end') {
							controller.abort();
						}
					} catch (error) {
						console.error('useSse parsing error');
					}
				},
				signal: controller.signal
			});
		}

		fetchInit();

		return () => controller.abort();
	}, []);

	return (
		<Fragment>
			<div className="chat">
				<header className="chat-header">Chat {isLoading ? 'Loading..' : ''}</header>
				<Messages/>
				<Footer/>
			</div>
		</Fragment>
	);
}
