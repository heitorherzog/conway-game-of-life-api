# Conway's Game of Life API 🧬

ASP.NET Core **.NET 9** service that exposes Conway's Game of Life as a REST API.
The application persists uploaded boards in **MongoDB** so that simulations survive
restarts, and it ships with Docker Compose to run the API and database together.

## 🎯 What does this service do?

Conway's Game of Life is a zero-player cellular automaton defined on a grid of
cells that live or die based on the state of their neighbours. This API lets you:

- Upload an initial board and receive a persistent identifier.
- Compute the next generation on demand.
- Jump forward _n_ generations without replaying every step.
- Detect the final state (steady state or oscillation) with a configurable
  safety limit that prevents unbounded processing.

Boards and intermediate generations are stored durably so the service can be
restarted without losing work.

---

## ⚙️ Tech stack

- **.NET 9** + **ASP.NET Core** for the HTTP layer.
- **MongoDB** (via `MongoDB.Driver`) for durable persistence of boards and
  generations.
- **Swagger / Swashbuckle** for interactive API exploration.
- **Docker & Docker Compose** for local orchestration of the API and MongoDB.

---

## 🚀 How to Run
- [.NET SDK 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (or Docker Engine)
- 
### 1. Clone the repository
- git clone https://github.com/heitorherzog/conway-game-of-life-api.git
- cd conway-game-of-life-api

### Option 1: Run everything with Docker Compose

```bash
docker compose up --build
```
### Option 2: Run on Visual studio, select the docker-compose file if not selected and hit F5
 
## 🔌 API quick reference

| Endpoint | Description |
| --- | --- |
| `POST /api/boards` | Upload a new board. Returns `{ "boardId": "..." }`. |
| `GET /api/boards/{boardId}/next` | Compute the next generation for a stored board. |
| `GET /api/boards/{boardId}/states?steps={n}` | Jump forward `n` generations and return each intermediate board. |
| `GET /api/boards/{boardId}/final?maxIterations={n}` | Return the final state, giving up after `n` iterations (defaults to 500). |

Boards are represented as two-dimensional integer arrays where `1` is alive and
`0` is dead. Example payload:

```json
{
  "cells": [
    [0, 1, 0],
    [0, 1, 0],
    [0, 1, 0]
  ]
}
```

---
