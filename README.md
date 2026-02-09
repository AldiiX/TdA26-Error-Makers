<div align="center">
  <h1 style="text-align: center">TdA26 – Error Makers</h1>
  <img src="https://img.shields.io/badge/Tour%20de%20App-2026-00dc82?style=for-the-badge" alt="Tour de App 2026" />
  <img src="https://img.shields.io/badge/status-in%20development-ffaa00?style=for-the-badge" alt="Status" />
</div>

<p align="center">
  Webová aplikace pro soutěž <b>Tour de App</b> – tým <b>Error Makers</b>.
</p>

<p align="center">
  Soutěžíme za <a href="https://educhem.cz"><b>Střední školu EDUCHEM, a.s.</b></a>
</p>

--- 

## Tým

<p align="center">
  <table>
    <tr>
      <td align="center" width="240">
        <img src="https://cloud0.emsio.cz/public/avatars/6f17f5a3-8d81-4f60-9260-ee69fb7a52d9.png" width="96" style="border-radius: 100%;" alt="Stanislav Škudrna" />
        <p></p>
        <a href="https://stanislavskudrna.cz" target="_blank"><b>AldiiX</b></a><br />
        <sub>Stanislav Škudrna<br />Fullstack · DevOps</sub>
      </td>
      <td align="center" width="240">
        <img src="https://cloud0.emsio.cz/public/avatars/38d32c1d-f592-4dd1-8238-3ac14fb7952e.png" width="96" style="border-radius: 100%;" alt="Serhii Yavorskyi" />
        <p></p>
        <a href="https://serhii.cz" target="_blank"><b>WezeAnonym</b></a><br />
        <sub>Serhii Yavorskyi<br />Fullstack · UI/UX</sub>
      </td>
      <td align="center" width="240">
        <img src="https://cloud0.emsio.cz/public/avatars/11c27d7cf633d7058554aba6b6941caa.webp" width="96" style="border-radius: 100%;" alt="Serhii Yavorskyi" />
        <p></p>
        <a href="https://jakubsokol.cz" target="_blank"><b>Jakooob</b></a><br />
        <sub>Jakub Sokol<br />Fullstack · Architecture</sub>
      </td>
    </tr>
  </table>
</p>

---

## Použité technologie

### Frontend

![Nuxtjs](https://img.shields.io/badge/Nuxt-002E3B?style=for-the-badge&logo=nuxtdotjs&logoColor=#00DC82)
![Vue.js](https://img.shields.io/badge/vuejs-%2335495e.svg?style=for-the-badge&logo=vuedotjs&logoColor=%234FC08D)
![SASS](https://img.shields.io/badge/SASS-hotpink.svg?style=for-the-badge&logo=SASS&logoColor=white)

### Backend

![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)

### Databáze

![MySQL](https://img.shields.io/badge/mysql-4479A1.svg?style=for-the-badge&logo=mysql&logoColor=white)
![Redis](https://img.shields.io/badge/redis-%23DD0031.svg?style=for-the-badge&logo=redis&logoColor=white)

---

## Jak spustit aplikaci

### 1. Naklonujte repozitář

```bash
git clone https://github.com/AldiiX/TdA26-Error-Makers.git
cd TdA26-Error-Makers
```

### 2. Instalace Dockeru

- **WSL (Windows)** – pokud ještě nemáte:
  ```bash
  wsl --install
  ```
- Instalace Docker Desktop:
  - [Windows](https://docs.docker.com/docker-for-windows/install/)
  - [Mac](https://docs.docker.com/docker-for-mac/install/)
  - [Linux](https://docs.docker.com/engine/install/)

### 3. Build Docker kontejneru

```bash
docker build . -t tda26-error-makers
```

### 4. Spuštění aplikace

```bash
docker run --name tda26-error-makers -p 80:80 tda26-error-makers
```

Aplikace poběží na adrese:

```text
http://localhost:80
```

### 5. Vypnutí a smazání kontejneru

```bash
docker stop tda26-error-makers && docker rm tda26-error-makers
```

---

## Pravidla commitování (předpony)

Pro konzistentní historii používáme tyto prefixy:

| Prefix     | Popis                                                                                           |
|-----------:|-------------------------------------------------------------------------------------------------|
| `FEAT`     | Přidána nová funkce                                                                             |
| `FIX`      | Oprava chyby                                                                                    |
| `CHORE`    | Změny nesouvisející s opravou nebo funkcí, které nemodifikují `src` ani testy (např. závislosti)|
| `REFACTOR` | Refaktorizace kódu, která neopravuje chybu ani nepřidává funkci                                 |
| `DOCS`     | Aktualizace dokumentace (README, další markdown soubory)                                        |
| `STYLE`    | Změny neovlivňující význam kódu (formátování, mezery, středníky, …)                             |
| `TEST`     | Přidání nových nebo oprava stávajících testů                                                    |
| `PERF`     | Vylepšení výkonu                                                                                |
| `CI`       | Změny týkající se kontinuální integrace                                                         |
| `REVERT`   | Návrat k předchozímu commitu                                                                    |

---

## Škola

> Soutěžíme za [Střední Školu EDUCHEM, a.s.](https://educhem.cz) – děkujeme za podporu :)
