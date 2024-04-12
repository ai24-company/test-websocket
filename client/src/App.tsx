import './App.scss';

import { Fragment, useEffect, useRef, useState } from 'react';
import { Message } from './components/message';

type DATA = {
	message: string;
	id: string;
	isMe: boolean;
	type: 'start' | 'stream' | 'end';
}

function App() {
	const [loading, setLoading] = useState(false);
	const [messages, setMessages] = useState<DATA[]>([]);
	const [message, setMessage] = useState('');
	const eventSource = useRef<EventSource | null>(null);

	const sendMessage = async () => {
		try {
			const response = await fetch('http://localhost:5238/send-text', {
				method: 'POST',
				headers: { 'content-type': 'application/json' },
				body: JSON.stringify({ message })
			});
			const json = await response.json();
			console.log(json);
			const es = new EventSource('http://localhost:5238/send-text');
			es.addEventListener('open', (event) => {
				console.log('Open source', event);
			});

			es.addEventListener('error', (event) => {
				console.log("Error:", event);
			});

			es.addEventListener('message', function (event) {
				const data = JSON.parse(event.data);
				console.log(data);
				if (data.type === 'stream') {
					setMessages(prevState => prevState.concat(data));
				}
				if (data.type === 'end') {
					this.close();
				}
			});
			console.log(json);
	//		setMessages(prevState => prevState.concat(json));
		} catch (error) {
			console.log('fetch Error', error);
		} finally {
			setMessage('');
		}
	};

	useEffect(() => {
		setLoading(true);
		eventSource.current = new EventSource('http://localhost:5238/create-stream');

		eventSource.current.addEventListener('open', (event) => {
			console.log('Open Stream:', event);
		});

		eventSource.current.addEventListener('error', (error) => {
			console.log("Error:", error);
		});

		eventSource.current.addEventListener('message', function (event) {
			const data = JSON.parse(event.data) as DATA;
			const typeMap = {
				'start': () => {
					setLoading(false);
				},
				'stream': () => {
					setMessages(prevState => prevState.concat(data));
				},
				'end': () => {
					this.close();
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
						<Message key={item.id} message={item.message} isMe={item.isMe}/>
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
