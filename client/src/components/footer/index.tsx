import { memo, useState } from 'react';

import { useActions } from '../../hooks/redux-hooks.ts';
import { type IncomingMessage } from '../../types';

export const Footer = memo(() => {
	const [message, setMessage] = useState('');
	const { messagesReceived } = useActions();

	const sendMessage = () => {
		const url = new URL(`${import.meta.env.VITE_API_URL}/send-text`);
		url.searchParams.set('typeChat', 'dialog');
		url.searchParams.set('incomeMessage', message);

		const source = new EventSource(url);
		source.addEventListener('open', (event) => {
			console.log('Open source', event);
		});

		source.addEventListener('error', (event) => {
			console.log('Error:', event);
		});

		source.addEventListener('message', function(event) {
			const data = JSON.parse(event.data) as IncomingMessage;
			console.log('Message Received: ', data);
			messagesReceived(data);

			if (data.sender === 'bot' && data.type === 'end') this.close();
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