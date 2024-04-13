// @ts-ignore
import express, { Request, Response } from 'express';
// @ts-ignore
import cors from 'cors';

const app = express();

app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: true, limit: '10mb' }));

app.set('port', process.env.PORT || 8000);

let data: any | null = null;

app.get('/start-stream', (req: Request, res: Response) => {
	res.setHeader('Content-Type', 'text/event-stream');
	res.setHeader('Cache-Control', 'no-cache');
	res.setHeader('Connection', 'keep-alive');

	const getData = (type: 'start' | 'stream' | 'end', id: number, message: string = '', isMe: boolean = false) => JSON.stringify({
		message,
		id,
		type,
		isMe
	});

	if (data) {
		console.log(data)
		res.write(`data: ${getData('start', data.id, '', true)}\n\n`);
		res.write(`data: ${getData('stream', data.id, data.message, true)}\n\n`);
		res.write(`data: ${getData('end', data.id, '', true)}\n\n`);
	} else {
		console.log('no data');
		res.write(`data: ${getData('start', 1)}\n\n`);
		res.write(`data: ${getData('stream', 1, 'Hello world')}\n\n`);
		res.write(`data: ${getData('end', 1)}\n\n`);
	}

	req.on('close', () => {
		console.log('Клиент закрыл соединение');
	});
});

app.post('/send-message', (req: Request, res: Response) => {
	const body = req.body;

	data = {};
	data['message'] = body;
	data['id'] = new Date().getTime();
	res.status(200).json({ status: 'ok' });
});

app.listen(app.get('port'), () => {
	console.log(`Server listening on port ${app.get('port')}`);
});