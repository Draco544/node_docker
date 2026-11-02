import WebSocket from 'ws';

const [servei, accio] = process.argv.slice(2);

if (!servei || !accio) {
    console.error('Ús: node client.js <servei> <accio>');
    process.exit(1);
}

const url = process.env.WS_URL || 'ws://localhost:3000';
const ws = new WebSocket(url);

ws.on('open', () => {
    ws.send(JSON.stringify({ servei, accio }));
});

ws.on('message', dades => {
    console.log(dades.toString());
    ws.close();
});

ws.on('error', error => {
    console.error(error.message);
    process.exit(1);
});
