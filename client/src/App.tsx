import { Fragment, useEffect, useRef, useState } from 'react';
import './App.scss';
import { Message } from './components/message';

const openWSHandle = (event: Event) => {
    console.log('openWSHandle', event);
}

const messageReceived = (event: MessageEvent) => {
    console.log('messageReceived', JSON.parse(event.data));
}

function App() {
    const [message, setMessage] = useState('');
    const ws = useRef<WebSocket | null>(null);

    useEffect(() => {
        ws.current = new WebSocket('ws://localhost:8000/api/chat');


        ws.current?.addEventListener('open', openWSHandle);
        ws.current?.addEventListener('message', messageReceived);

        return () => {
            ws.current?.removeEventListener('open', openWSHandle);
        }
    }, []);

    const sendMessage = () => {
        ws.current?.send(JSON.stringify({ message }));
    }

	return (
		<Fragment>
			<div className="chat">
				<header className="chat-header">Chat</header>
				<main className="chat-body">
					<Message message="Hello World!" isMe={true}/>
					<Message message="Hello World!" isMe={false}/>
					<Message message="Hello World!" isMe={true}/>
					<Message message="Hello World!" isMe={false}/>
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
