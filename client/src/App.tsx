import './App.scss';

import { Fragment, useEffect, useRef, useState } from 'react';
import { Footer } from './components/footer';
import { useActions } from './hooks/redux-hooks.ts';
import { Messages } from './components/messages';
import { type IncomingMessage } from './types';

function App() {
	const [loading, setLoading] = useState(false);
	const eventSource = useRef<EventSource | null>(null);
	const { messagesReceived } = useActions();

	useEffect(() => {
		const url = new URL(`${import.meta.env.VITE_API_URL}/send-text`);
		url.searchParams.set('incomeMessage', '');
		url.searchParams.set('typeChat', 'init');

		setLoading(true);
		eventSource.current = new EventSource(url);

		eventSource.current.addEventListener('open', (event) => {
			console.log('Open Stream:', event);
		});

		eventSource.current.addEventListener('error', (error) => {
			console.log('Error:', error);
		});

		eventSource.current.addEventListener('message', function(event) {
			const data = JSON.parse(event.data) as IncomingMessage;
			console.log('Message Received: ', data);
			messagesReceived(data);

			if (data.type === 'end') this.close();
		});

		return () => {
			eventSource.current?.removeEventListener('open', function(event) {
				console.log('Remove Open Source', event);
				this.close();
			});
		};
	}, []);

	return (
		<Fragment>
			<div className="chat">
				<header className="chat-header">Chat {loading ? 'Loading..' : ''}</header>
				<Messages/>
				<Footer/>
			</div>
		</Fragment>
	);
}

export default App;
