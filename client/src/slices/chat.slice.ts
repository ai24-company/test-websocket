import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { type IncomingMessage } from '../types';

export interface StateOptions {
	messages: IncomingMessage[];
}

const initialState: StateOptions = {
	messages: [],
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
	},
});

export { chatActions, chatReducer };