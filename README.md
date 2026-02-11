# GenericWebSocket v1.0.0

Uma biblioteca simples e tipada para comunicação via **Socket.IO Client** em aplicações **.NET 8 (Console / Worker / Service)**, com suporte a **ILogger**, **DI**, **factory** e **registro de eventos fortemente tipados**.

---

## 🎯 Objetivo

O objetivo deste projeto é fornecer uma abstração limpa e reutilizável para:

* Inicializar conexões Socket.IO
* Emitir eventos
* Registrar eventos tipados (`RegisterNewEvent<T>`)
* Centralizar logging
* Evitar eventos duplicados
* Manter o código desacoplado da aplicação principal

---

## 🧱 Arquitetura

O projeto é composto por dois elementos principais:

* **GenericWebSocket** → Serviço responsável pela comunicação Socket.IO
* **Factory (DI)** → Responsável por criar a instância passando `route` e `options`

O logger é sempre injetado via **Dependency Injection**, mantendo o serviço testável e desacoplado.

---

## 🚀 Exemplo de Uso

### Program.cs

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
        });

        services.AddSingleton<GenericWebSocket>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<GenericWebSocket>>();

            var options = new SocketIOOptions
            {
                Reconnection = true,
                ReconnectionAttempts = 5
            };

            return new GenericWebSocket(
                route: "http://localhost:3000",
                logger: logger,
                options: options
            );
        });
    })
    .Build();

var socket = host.Services.GetRequiredService<GenericWebSocket>();

await socket.InitializeSocket();

socket.RegisterNewEvent<string>("message", msg =>
{
    Console.WriteLine($"Mensagem recebida: {msg}");
});

await socket.EmitEvent("ping", "hello server");
```

---

## 🧠 GenericWebSocket — Métodos e Responsabilidades

```csharp
/// <summary>
/// Cria uma nova instância do cliente Socket.IO com rota, logger e opções opcionais.
/// </summary>
public GenericWebSocket(string route, ILogger<GenericWebSocket> logger, SocketIOOptions? options = null)
```

```csharp
/// <summary>
/// Inicializa a conexão com o servidor Socket.IO e registra o listener global de eventos.
/// </summary>
public async Task<bool> InitializeSocket()
```

```csharp
/// <summary>
/// Emite um evento para o servidor com payload opcional.
/// </summary>
public async Task EmitEvent(string eventName, params object[] payload)
```

```csharp
/// <summary>
/// Registra um evento tipado garantindo que não seja duplicado.
/// </summary>
public void RegisterNewEvent<T>(string eventName, Action<T> callback)
```

```csharp
/// <summary>
/// Encerra a conexão com o servidor Socket.IO de forma assíncrona.
/// </summary>
public async Task UninitializeSocketAsync()
```

---

## 🏭 Factory (DI) — Responsabilidade

```csharp
/// <summary>
/// Cria e registra a instância do GenericWebSocket com parâmetros dinâmicos via DI.
/// </summary>
services.AddSingleton<GenericWebSocket>(sp => { ... });
```

A factory permite:

* Definir `route` dinamicamente
* Customizar `SocketIOOptions`
* Garantir que o `ILogger` seja corretamente injetado

---

## 📌 Decisões de Design

* ❌ Classe estática → evita estado global
* ✅ Logger injetado → controle total de logs
* ✅ HashSet para eventos → evita duplicidade
* ✅ OnAny → fallback para eventos não registrados
* ✅ Tipagem genérica → segurança e clareza

---

## 📦 Versão

**v1.0.0**

Primeira versão estável com:

* Conexão
* Emissão
* Registro tipado de eventos
* Logging integrado
* Integração com DI

---

## 🔮 Próximos Passos (Opcional)

* Interface pública (`IGenericWebSocket`)
* Callbacks assíncronos (`Func<T, Task>`)
* Reconexão customizada por evento
* Testes unitários

---

## 👤 Autor

Samuel

Projeto criado com foco em aprendizado real, boas práticas e código reutilizável.
