import './App.scss';

import { Fragment, useEffect, useRef, useState } from 'react';
import { Message } from './components/message';

type DATA = {
	message: string;
	id: string;
	sender: 'user' | 'bot';
	type: 'start' | 'stream' | 'end';
	typeChat: 'init' | 'dialog'
}

function App() {
	const [loading, setLoading] = useState(false);
	const [messages, setMessages] = useState<DATA[]>([]);
	const [message, setMessage] = useState('');
	const eventSource = useRef<EventSource | null>(null);

	const sendMessage = async () => {
		setLoading(true);
		const url = new URL('http://localhost:5238/send-text');
		url.searchParams.set('typeChat', 'dialog');
		url.searchParams.set('incomeMessage', message);

		try {
			const source = new EventSource(url);
			source.addEventListener('open', (event) => {
				console.log('Open source', event);
			});

			source.addEventListener('error', (event) => {
				console.log("Error:", event);
			});

			source.addEventListener('message', function (event) {
				const data = JSON.parse(event.data) as DATA;
				console.log('Message Received: ', data);
				const typeMap = {
					'start': () => {
						setLoading(false);
					},
					'stream': () => {
						setMessages(prevState => prevState.concat(data));
					},
					'end': () => {
						if (data.sender === 'bot')
							this.close();
						setLoading(false);
					},
				};

				typeMap[data.type]?.();
			});
		} catch (error) {
			console.log('fetch Error', error);
		} finally {
			setMessage('');
		}
	};

	useEffect(() => {
		const url = new URL('http://localhost:5238/send-text');
		url.searchParams.set('incomeMessage', '');
		url.searchParams.set('typeChat', 'init');

		setLoading(true);
		eventSource.current = new EventSource(url);

		eventSource.current.addEventListener('open', (event) => {
			console.log('Open Stream:', event);
		});

		eventSource.current.addEventListener('error', (error) => {
			console.log("Error:", error);
		});

		eventSource.current.addEventListener('message', function (event) {
			const data = JSON.parse(event.data) as DATA;
			console.log('Message Received: ', data);

			const typeMap = {
				'start': () => {
					setLoading(false);
				},
				'stream': () => {
					setMessages(prevState => {
						const existingMessageIndex = prevState.findIndex(({ id }) => id === data.id);

						if (existingMessageIndex !== -1) {
							prevState[existingMessageIndex].message += data.message;
						} else {
							prevState.push(data);
						}

						return prevState;
					});
				},
				'end': () => {
					if (data.sender === 'bot') {
						this.close();
					}
				},
			};

			typeMap[data.type]?.();
		});

		return () => {
			eventSource.current?.removeEventListener('open', function (event) {
				console.log('Remove Open Source', event);
				this.close();
			})
		}
	}, []);

	return (
		<Fragment>
			<div className="chat">
				<header className="chat-header">Chat {loading ? 'Loading..': ''}</header>
				<main className="chat-body">
					{messages?.map((item) => (
						<Message key={item.id} message={item.message} isMe={item.sender === 'user'}/>
					))}
				</main>
				<footer className="chat-footer">
					<textarea className="textarea" value={message} onChange={({ target }) => setMessage(target.value)}/>
					<button className="send-btn" type="button" onClick={sendMessage}>
						Send
					</button>
				</footer>
			</div>
		</Fragment>
	);
}

export default App;
