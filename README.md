# node_docker — Node.js + Docker + SSH con cliente Unity

Gestor de servicios por WebSocket. Un servidor **Node.js** recibe órdenes y ejecuta **Docker** en local o por **SSH**. Incluye un cliente **Unity** con interfaz para controlar los servicios.

> README temporal de presentación del repositorio.

## Estructura

```
node_docker/
├── servidor.js            # Servidor WebSocket que ejecuta Docker
├── client.js              # Cliente de consola para probar sin Unity
├── main.js                # Punto de entrada auxiliar (vacío)
├── docker-compose.yml     # Contenedores de prueba (api y worker)
├── services.json          # Servicios disponibles y su host
├── package.json
├── package-lock.json
├── .env.example
└── unity/                 # Proyecto Unity (Assets, Packages, ProjectSettings)
    ├── Assets/
    │   ├── Scenes/SampleScene.unity
    │   ├── Scripts/WebSocketClient.cs
    │   └── Scripts/MenuController.cs
    ├── Packages/
    └── ProjectSettings/
```

## Requisitos

- **Node.js** 20 o superior (probado con Node 26)
- **Docker** y **Docker Compose**
- **Unity** 6000.6.0f1 (solo para el cliente gráfico)

## Puesta en marcha del servidor

1. Entra en la carpeta del proyecto:

   ```bash
   cd Node_docker_SSH
   ```

2. Instala las dependencias (si tienes `npm`):

   ```bash
   npm install
   ```

3. Crea el archivo `.env` a partir del ejemplo:

   ```bash
   cp .env.example .env
   ```

4. Levanta los contenedores de prueba:

   ```bash
   docker compose up -d
   ```

5. Arranca el servidor:

   ```bash
   npm start
   # o, sin npm:
   node --env-file=.env servidor.js
   ```

El servidor escucha en `ws://localhost:3000`.

## Probar sin Unity

Con el servidor en marcha, en otra terminal:

```bash
node client.js servei-api estat
node client.js servei-api logs
node client.js servei-api parar
node client.js servei-api iniciar
node client.js servei-api reiniciar
```

También puedes usar `servei-worker` como primer argumento.

## Cliente Unity

1. Abre la carpeta `unity/` con **Unity 6000.6.0f1**.
2. Abre la escena `Assets/Scenes/SampleScene.unity`.
3. Pulsa **Play**.
4. Usa los botones de cada servicio.

La URL del WebSocket se configura en el campo `websocketUrl` del componente `WebSocketClient` (por defecto `ws://localhost:3000`).

El proyecto usa la dependencia `com.endel.nativewebsocket` (declarada en `unity/Packages/manifest.json`).

## Servicios y acciones

Los servicios se definen en `services.json`:

```json
{
  "servei-api": { "host": "local", "container": "api" },
  "servei-worker": { "host": "local", "container": "worker" }
}
```

Acciones permitidas: `iniciar`, `parar`, `reiniciar`, `estat` y `logs`.

## Servicio remoto por SSH

Añade una entrada en `services.json`:

```json
"servei-api-remot": { "host": "ssh://alumne@192.168.1.101", "container": "api" }
```

Comprueba la conexión antes de usarlo:

```bash
docker -H ssh://alumne@192.168.1.101 ps
```

Para automatizar la contraseña en pruebas se puede usar `sshpass`, pero no es recomendable en producción. Es mejor configurar una clave con `ssh-keygen` y `ssh-copy-id`.

## Detener la demo

```bash
docker compose down
```
