import { WebSocketServer, WebSocket } from 'ws';
import { spawn } from 'node:child_process';
import { readFileSync } from 'node:fs';

const serveis = JSON.parse(readFileSync(process.env.SERVICES_FILE, 'utf8'));
const port = Number(process.env.PORT);
const wss = new WebSocketServer({ port });
const ordres = { iniciar: 'start', parar: 'stop', reiniciar: 'restart' };

wss.on('connection', ws => {
    ws.on('message', raw => {
        let peticio;
        try {
            peticio = JSON.parse(raw.toString());
        } catch {
            return ws.send(JSON.stringify({ error: 'JSON incorrecte' }));
        }

        const { servei, accio } = peticio;
        const cfg = Object.hasOwn(serveis, servei) ? serveis[servei] : null;

        if (!cfg || ![...Object.keys(ordres), 'estat', 'logs'].includes(accio)) {
            return ws.send(JSON.stringify({ error: 'Servei o acció no permesos' }));
        }

        const prefix = cfg.host === 'local' ? [] : ['-H', cfg.host];
        const args = accio === 'estat' ? ['inspect', '-f', '{{.State.Status}}', cfg.container]
            : accio === 'logs' ? ['logs', '--tail', '30', cfg.container]
            : [ordres[accio], cfg.container];

        const child = spawn('docker', [...prefix, ...args]);
        let sortida = '';
        let errors = '';
        let respost = false;

        const respondre = dades => {
            if (respost) return;
            respost = true;
            if (ws.readyState === WebSocket.OPEN) {
                ws.send(JSON.stringify(dades));
            }
        };

        child.stdout.on('data', x => sortida += x.toString());
        child.stderr.on('data', x => errors += x.toString());
        child.on('error', e => respondre({ servei, accio, error: e.message }));
        child.on('close', codi => respondre({
            servei,
            accio,
            codi,
            sortida: sortida.trim(),
            errors: errors.trim()
        }));
    });
});

console.log(`Servidor WebSocket a ws://localhost:${port}`);
