import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { type IncomingMessage } from '../types';

export interface StateOptions {
	messages: IncomingMessage[];
	isLoading: boolean;
	token: string;
}

const initialState: StateOptions = {
	messages: [],
	isLoading: false,
	token: crypto.randomUUID()
};

const { actions: chatActions, reducer: chatReducer } = createSlice({
	name: 'chat',
	initialState,
	reducers: {
		messagesReceived: (state, { payload }: PayloadAction<IncomingMessage>) => {
			switch (payload.type) {
				case 'start': {
					break;
				}
				case 'stream': {
					const existingMessageIndex = state.messages.findIndex(({ id }) => id === payload.id);

					if (existingMessageIndex !== -1) {
						state.messages[existingMessageIndex].message += payload.message;
					} else {
						state.messages.push(payload);
					}
					break;
				}
				case 'end': {
					break;
				}
			}
		},
		deleteMessages: (state) => {
			state.messages = [];
		},
		addMessage: (state, { payload }: PayloadAction<IncomingMessage>) => {
			state.messages.push(payload);
		},
		toggleLoading: (state, { payload }: PayloadAction<boolean>) => {
			state.isLoading = payload;
		}
	},
});

export { chatActions, chatReducer };