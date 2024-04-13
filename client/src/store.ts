import { combineReducers } from 'redux';
import { configureStore } from '@reduxjs/toolkit';

import { chatReducer } from './slices/chat.slice';

const rootReducer = combineReducers({
	chat: chatReducer,
});

export const store = configureStore({
	reducer: rootReducer,
	middleware: (getDefaultMiddleware) => getDefaultMiddleware(),
});

export type AppDispatch = typeof store.dispatch;
export type RootState = ReturnType<typeof store.getState>;