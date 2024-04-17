import { memo, useState } from 'react';
import { EventStreamContentType, fetchEventSource } from '@microsoft/fetch-event-source';

import { useActions, useAppSelector } from '../../hooks/redux-hooks.ts';
import { type IncomingMessage } from '../../types';

class RetriableError extends Error { }
class FatalError extends Error { }

export const Footer = memo(() => {
	const [message, setMessage] = useState('');
	const token = useAppSelector(state => state.chat.token);
	const { messagesReceived, toggleLoading } = useActions();

	const sendMessage = async () => {
		const controller = new AbortController();
		const url = `${import.meta.env.VITE_API_URL}/send-text`;

		await fetchEventSource(url, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
			body: JSON.stringify({
				incomeMessage: message,
				token,
				typeChat: 'dialog'
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