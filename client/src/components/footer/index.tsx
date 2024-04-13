import { memo, useState } from 'react';
import { fetchEventSource } from '@microsoft/fetch-event-source';

import { useActions } from '../../hooks/redux-hooks.ts';
import { type IncomingMessage } from '../../types';

export const Footer = memo(() => {
	const [message, setMessage] = useState('');
	const { messagesReceived } = useActions();

	const sendMessage = async () => {
		const controller = new AbortController();
		const url = new URL(`${import.meta.env.VITE_API_URL}/send-text`);

		url.searchParams.set('typeChat', 'dialog');
		url.searchParams.set('incomeMessage', message);

		await fetchEventSource(url.toString(), {
			onmessage(event) {
				try {
					const parsedData = JSON.parse(event.data) as IncomingMessage;
					messagesReceived(parsedData);
					if (parsedData.sender === 'bot' && parsedData.type === 'end') {
						controller.abort();
					}
				} catch(error) {
					console.error('useSse parsing error');
				}
			},
			signal: controller.signal
		});
	};

	return (
		<footer className="chat-footer">
			<textarea className="textarea" value={message} onChange={({ target }) => setMessage(target.value)}/>
			<button className="send-btn" type="button" onClick={sendMessage}>
				Send
			</button>
		</footer>
	);
});