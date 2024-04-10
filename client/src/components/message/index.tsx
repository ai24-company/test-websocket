import './message.scss';
import cn from 'classnames';

export const Message = (props: Props) => {
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
}

interface Props {
	isMe: boolean;
	message: string;
}