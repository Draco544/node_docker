using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NativeWebSocket;

public class WebSocketClient : MonoBehaviour
{
    [SerializeField]
    private string websocketUrl = "ws://localhost:3000";

    private Action<string> onMessage;
    private Action onOpen;

    WebSocket websocket;

    async void Start()
    {
        await ConnectWebSocket();
    }

    private async Task ConnectWebSocket()
    {
        websocket = new WebSocket(websocketUrl);

        websocket.OnOpen += () =>
        {
            Debug.Log("WebSocket connectat!");
            onOpen?.Invoke();
        };

        websocket.OnError += e =>
        {
            Debug.LogError("WebSocket error: " + e);
        };

        websocket.OnClose += e =>
        {
            Debug.Log("WebSocket tancat!");
        };

        websocket.OnMessage += bytes =>
        {
            string message = Encoding.UTF8.GetString(bytes);
            Debug.Log("Missatge rebut: " + message);
            onMessage?.Invoke(message);
        };

        await websocket.Connect();
    }

    public async void SendWebSocketMessage(string message)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.SendText(message);
        }
        else
        {
            Debug.LogWarning("No es pot enviar el missatge: connexió no oberta.");
        }
    }

    void Update()
    {
        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            await websocket.Close();
        }
    }

    public void SetMessageHandler(Action<string> handler)
    {
        onMessage = handler;
    }

    public void SetOpenHandler(Action handler)
    {
        onOpen = handler;

        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            onOpen?.Invoke();
        }
    }
}
