import { Fragment, useEffect, useRef, useState } from 'react';
import './App.scss';
import { Message } from './components/message';

// type DATA = {
// 	message: string;
// 	id: string;
// 	isMe: boolean;
// }

function App() {
	const [messages, setMessages] = useState<any[]>([]);
    const [message, setMessage] = useState('');
    const ES = useRef<EventSource | null>(null);

	const sendMessage = async () => {
		try {
			const response = await fetch('http://localhost:5238/send-text', {
				method: 'POST',
				headers: {'content-type': 'application/json'},
				body: JSON.stringify({message})
			});
			const json = await response.json();
			setMessages(prevState => prevState.concat(json));
		}
		catch (error) {
			console.log('fetch Error', error);
		}
		finally {
			setMessage('');
		}
	}

    useEffect(() => {
	    ES.current = new EventSource('http://localhost:5238/');

		ES.current.onopen = (event) => {
			console.log('Open source', event);
		};

		ES.current.onerror = (event) => {
			console.log("Error:", event);
		};

	    ES.current.onmessage = (event) => {
		    console.log("New item received:", event);
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
					{messages?.map((item, index) => (
						<Message key={index} message={item} isMe={false} />
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
