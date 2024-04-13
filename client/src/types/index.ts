export type IncomingMessage = {
	message: string;
	id: string;
	sender: 'user' | 'bot';
	type: 'start' | 'stream' | 'end';
}