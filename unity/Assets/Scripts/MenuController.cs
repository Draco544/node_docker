using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class Peticio
{
    public string servei;
    public string accio;

    public Peticio(string servei, string accio)
    {
        this.servei = servei;
        this.accio = accio;
    }
}

[Serializable]
public class Resposta
{
    public string servei;
    public string accio;
    public int codi;
    public string sortida;
    public string errors;
    public string error;
}

public class MenuController : MonoBehaviour
{
    [SerializeField]
    private UIDocument menu;

    [SerializeField]
    private WebSocketClient webSocketClient;

    private VisualElement root;
    private ScrollView logsPanel;
    private Label logsTitle;
    private readonly Dictionary<string, Label> estats = new Dictionary<string, Label>();

    void Start()
    {
        root = menu.rootVisualElement;
        logsPanel = root.Q<ScrollView>("logsPanel");
        logsTitle = root.Q<Label>("logsTitle");

        ConfigurarServei("servei-api", "api");
        ConfigurarServei("servei-worker", "worker");

        webSocketClient.SetMessageHandler(OnMessage);
        webSocketClient.SetOpenHandler(OnOpen);
    }

    private void ConfigurarServei(string servei, string prefix)
    {
        Label estat = root.Q<Label>(prefix + "State");
        estats[servei] = estat;

        root.Q<Button>(prefix + "Start").clicked += () => Enviar(servei, "iniciar");
        root.Q<Button>(prefix + "Stop").clicked += () => Enviar(servei, "parar");
        root.Q<Button>(prefix + "Restart").clicked += () => Enviar(servei, "reiniciar");
        root.Q<Button>(prefix + "Status").clicked += () => Enviar(servei, "estat");
        root.Q<Button>(prefix + "Logs").clicked += () => Enviar(servei, "logs");
    }

    private void OnOpen()
    {
        Enviar("servei-api", "estat");
        Enviar("servei-worker", "estat");
    }

    private void Enviar(string servei, string accio)
    {
        Peticio peticio = new Peticio(servei, accio);
        webSocketClient.SendWebSocketMessage(JsonUtility.ToJson(peticio));
    }

    private void OnMessage(string missatge)
    {
        Resposta resposta;

        try
        {
            resposta = JsonUtility.FromJson<Resposta>(missatge);
        }
        catch
        {
            AfegirLog("Resposta incorrecta: " + missatge);
            return;
        }

        if (resposta == null)
        {
            AfegirLog("Resposta incorrecta: " + missatge);
            return;
        }

        if (!string.IsNullOrEmpty(resposta.error))
        {
            AfegirLog("Error: " + resposta.error);
            return;
        }

        if (resposta.accio == "estat")
        {
            if (estats.TryGetValue(resposta.servei, out Label estat))
            {
                string text = string.IsNullOrEmpty(resposta.sortida) ? resposta.errors : resposta.sortida;
                estat.text = TextEstat(text);
                estat.style.color = resposta.codi != 0 ? Color.red : ColorEstat(text);
            }
            return;
        }

        if (resposta.accio == "logs")
        {
            MostrarLogs(resposta);
            return;
        }

        AfegirLog($"{resposta.servei}: {resposta.accio} (codi {resposta.codi})");
        Enviar(resposta.servei, "estat");
    }

    private string TextEstat(string estat)
    {
        switch (estat)
        {
            case "running": return "En marxa";
            case "exited": return "Aturat";
            case "restarting": return "Reiniciant";
            case "paused": return "Pausat";
            case "created": return "Creat";
            default: return string.IsNullOrEmpty(estat) ? "desconegut" : estat;
        }
    }

    private Color ColorEstat(string estat)
    {
        switch (estat)
        {
            case "running": return Color.green;
            case "exited": return Color.red;
            case "restarting": return Color.yellow;
            case "paused": return Color.yellow;
            case "created": return Color.cyan;
            default: return Color.gray;
        }
    }

    private void MostrarLogs(Resposta resposta)
    {
        logsTitle.text = "Logs: " + resposta.servei;
        logsPanel.Clear();

        string text = string.IsNullOrEmpty(resposta.sortida) ? resposta.errors : resposta.sortida;

        if (string.IsNullOrEmpty(text))
        {
            text = "(sense logs)";
        }

        foreach (string linia in text.Split('\n'))
        {
            Label label = new Label(linia);
            label.style.color = Color.white;
            logsPanel.Add(label);
        }

        BaixarScroll();
    }

    private void AfegirLog(string missatge)
    {
        string hora = DateTime.Now.ToString("HH:mm:ss");
        Label label = new Label($"[{hora}] {missatge}");
        label.style.color = Color.white;
        logsPanel.Add(label);
        BaixarScroll();
    }

    private void BaixarScroll()
    {
        logsPanel.schedule.Execute(() =>
        {
            logsPanel.scrollOffset = new Vector2(0, logsPanel.contentContainer.layout.height);
        });
    }
}
