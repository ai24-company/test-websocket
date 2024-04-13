import { memo } from 'react';

import { useAppSelector } from '../../hooks/redux-hooks.ts';
import { Message } from '../message';

export const Messages = memo(() => {
	const messages = useAppSelector(state => state.chat.messages);

	return (
		<main className="chat-body">
			{messages?.map((item) => (
				<Message
					key={item.id}
					message={item.message}
					isMe={item.sender === 'user'}
				/>
			))}
		</main>
	);
});