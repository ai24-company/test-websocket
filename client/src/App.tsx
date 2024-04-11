import { Fragment, useEffect, useRef, useState } from 'react';
import './App.scss';
// import { Message } from './components/message';

function App() {
    const [message, setMessage] = useState('');
    const ES = useRef<EventSource | null>(null);

	const sendMessage = async () => {
		const response = await fetch('http://localhost:5238/send-text', {
			method: 'POST',
			headers: {'content-type': 'application/json'},
			body: JSON.stringify({message})
		});
		const json = await response.json();
		setMessage(json);
	}

    useEffect(() => {
	    ES.current = new EventSource('http://localhost:5238/');

		ES.current?.addEventListener('error', (event) => {
			console.log("Error:", event);
		});

	    ES.current.onmessage = (event) => {
		    const item = JSON.parse(event.data);
		    console.log("New item received:", item);
	    };

        return () => {
            ES.current?.close();
        }
    }, []);

	return (
		<Fragment>
			<div className="chat">
				<header className="chat-header">Chat</header>
				<main className="chat-body">
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
