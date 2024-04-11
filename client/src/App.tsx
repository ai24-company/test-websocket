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
		eventSource.current = new EventSource('http://localhost:5238/');

		console.log(eventSource.current);

		eventSource.current.addEventListener('open', (event) => {
			console.log('Open source', event);
		});

		eventSource.current.addEventListener('error', (event) => {
			console.log("Error:", event);
		});

		eventSource.current.addEventListener('message', function (event) {
			const data = JSON.parse(event.data);
			if (data.type === 'stream') {
				setMessages(prevState => prevState.concat(data));
			}
			if (data.type === 'end') {
				this.close();
			}
		});

		return () => {
			eventSource.current?.removeEventListener('open', function (event) {
				console.log('Remove Open Source', event);
				this.close();
			})
		}
	}, []);

	// useEffect(() => {
	// 	const fetching = async () => {
	// 		try {
	// 			console.log('TRY START');
	//
	// 			const response = await fetch('http://localhost:5238/', {
	// 				method: 'POST',
	// 				// headers: { 'Content-Type': 'text/event-stream' },
	// 				body: JSON.stringify({ 'user_id': 123 }),
	// 			});
	//
	// 			if (!response.body) return;
	//
	// 			const reader = response.body.getReader();
	// 			while (true) {
	// 				const { done, value } = await reader.read();
	// 				if (done) return;
	// 				console.log(value)
	//
	// 			}
	// 		} catch (error) {
	// 			console.log('CATCH: ', error);
	// 		} finally {
	// 			console.log('FINALLY');
	// 		}
	// 	};
	//
	// 	fetching();
	// }, []);

	return (
		<Fragment>
			<div className="chat">
				<header className="chat-header">Chat</header>
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
