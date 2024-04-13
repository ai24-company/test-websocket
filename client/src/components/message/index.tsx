import './message.scss';
import cn from 'classnames';
import { memo } from 'react';

export const Message = memo((props: Props) => {
	const { message, isMe } = props;

	return (
		<div className={cn("chat-message", isMe ? "user" : "bot")}>
			<span className="chat-message__icon">
				{isMe ? 'User' : 'Bot'}
			</span>
			<span className="chat-message__separator"/>
			<div className="chat-message__message">
				{message}
			</div>
		</div>
	)
});

interface Props {
	isMe: boolean;
	message: string;
}